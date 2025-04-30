// Analysis/Roslyn/Analyzers/EntityAnalyzer.cs
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MPR.CodeGenTool.Domain.Metadata.Common;
using MPR.CodeGenTool.Domain.Metadata.Entity;

namespace MPR.CodeGenTool.Analysis.Roslyn.Analyzers
{
    public class EntityAnalyzer : IRoslynAnalyzer<EntityMetadata>
    {
        public bool CanAnalyze(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDeclaration && 
                   !classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        }

        public EntityMetadata Analyze(SyntaxNode node)
        {
            var classDeclaration = (ClassDeclarationSyntax)node;
            var semanticModel = classDeclaration.SyntaxTree.GetSemanticModel();
            
            var entity = new EntityMetadata
            {
                Name = classDeclaration.Identifier.Text,
                IsPartial = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
                IsPublic = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)),
                Properties = new List<PropertyMetadata>(),
                NavigationProperties = new List<NavigationPropertyMetadata>(),
                Attributes = ExtractAttributes(classDeclaration.AttributeLists)
            };
            
            foreach (var member in classDeclaration.Members)
            {
                if (member is PropertyDeclarationSyntax property)
                {
                    AnalyzeProperty(property, entity, semanticModel);
                }
            }
            
            return entity;
        }
        
        private void AnalyzeProperty(PropertyDeclarationSyntax property, EntityMetadata entity, SemanticModel semanticModel)
        {
            var attributes = ExtractAttributes(property.AttributeLists);
            
            bool isKey = attributes.Any(a => a.Name == "Key");
            bool isDatabaseGenerated = attributes.Any(a => a.Name == "DatabaseGenerated");
            bool isRequired = attributes.Any(a => a.Name == "Required");
            
            var propertySymbol = semanticModel.GetDeclaredSymbol(property);
            var propertyType = propertySymbol.Type;
            
            // Determine if it's a navigation property
            if (IsNavigationProperty(propertyType, semanticModel))
            {
                var navProperty = new NavigationPropertyMetadata
                {
                    Name = property.Identifier.Text,
                    Type = property.Type.ToString(),
                    IsVirtual = property.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)),
                    Attributes = attributes,
                    NavigationType = DetermineNavigationType(propertyType),
                    TargetEntityName = ExtractTargetEntityName(propertyType)
                };
                
                entity.NavigationProperties.Add(navProperty);
            }
            else
            {
                var prop = new PropertyMetadata
                {
                    Name = property.Identifier.Text,
                    Type = property.Type.ToString(),
                    IsKey = isKey,
                    IsDatabaseGenerated = isDatabaseGenerated,
                    IsRequired = isRequired,
                    IsVirtual = property.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)),
                    Attributes = attributes
                };
                
                if (isDatabaseGenerated)
                {
                    prop.DatabaseGeneratedOption = ExtractDatabaseGeneratedOption(attributes);
                }
                
                entity.Properties.Add(prop);
            }
        }
        
        private List<AttributeMetadata> ExtractAttributes(SyntaxList<AttributeListSyntax> attributeLists)
        {
            var result = new List<AttributeMetadata>();
            
            foreach (var attributeList in attributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name.ToString();
                    if (attributeName.EndsWith("Attribute"))
                    {
                        attributeName = attributeName.Substring(0, attributeName.Length - 9);
                    }
                    
                    var attributeMetadata = new AttributeMetadata
                    {
                        Name = attributeName,
                        Arguments = ExtractAttributeArguments(attribute)
                    };
                    
                    result.Add(attributeMetadata);
                }
            }
            
            return result;
        }
        
        private List<AttributeArgumentMetadata> ExtractAttributeArguments(AttributeSyntax attribute)
        {
            var result = new List<AttributeArgumentMetadata>();
            
            if (attribute.ArgumentList != null)
            {
                foreach (var argument in attribute.ArgumentList.Arguments)
                {
                    var argMetadata = new AttributeArgumentMetadata
                    {
                        Value = argument.Expression.ToString(),
                        IsNamedArgument = argument.NameEquals != null
                    };
                    
                    if (argument.NameEquals != null)
                    {
                        argMetadata.Name = argument.NameEquals.Name.ToString();
                    }
                    
                    result.Add(argMetadata);
                }
            }
            
            return result;
        }
        
        private bool IsNavigationProperty(ITypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            // Check if it's a collection type (ICollection<T>, List<T>, etc.)
            if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                var genericType = namedType.TypeArguments.First();
                return IsEntityType(genericType, semanticModel);
            }
            
            // Or a direct reference to another entity
            return IsEntityType(typeSymbol, semanticModel);
        }
        
        private bool IsEntityType(ITypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            // This is a simplistic check - in a real implementation you would
            // need to check if the type is defined in your domain project,
            // has properties with [Key] attributes, etc.
            return typeSymbol.TypeKind == TypeKind.Class && 
                   !typeSymbol.IsValueType &&
                   typeSymbol.Name != "string" &&
                   !typeSymbol.IsPrimitive();
        }
        
        private NavigationPropertyType DetermineNavigationType(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                // Check if it's ICollection<T>, List<T>, etc.
                var typeName = namedType.Name;
                if (typeName == "ICollection" || typeName == "List" || typeName == "HashSet" ||
                    typeName == "IEnumerable" || typeName == "IList")
                {
                    return NavigationPropertyType.OneToMany;
                }
            }
            
            return NavigationPropertyType.ManyToOne; // Default for reference types
        }
        
        private string ExtractTargetEntityName(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                return namedType.TypeArguments.First().Name;
            }
            
            return typeSymbol.Name;
        }
        
        private DatabaseGeneratedOption ExtractDatabaseGeneratedOption(List<AttributeMetadata> attributes)
        {
            var dbGenAttribute = attributes.FirstOrDefault(a => a.Name == "DatabaseGenerated");
            if (dbGenAttribute != null && dbGenAttribute.Arguments.Count > 0)
            {
                var optionArg = dbGenAttribute.Arguments.First().Value;
                if (optionArg.Contains("Identity"))
                    return DatabaseGeneratedOption.Identity;
                if (optionArg.Contains("Computed"))
                    return DatabaseGeneratedOption.Computed;
            }
            
            return DatabaseGeneratedOption.None;
        }
    }
}