// Analysis/Roslyn/Analyzers/DbContextAnalyzer.cs
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MPR.CodeGenTool.Domain.Metadata.Common;
using MPR.CodeGenTool.Domain.Metadata.Context;

namespace MPR.CodeGenTool.Analysis.Roslyn.Analyzers
{
    public class DbContextAnalyzer : IRoslynAnalyzer<DbContextMetadata>
    {
        private readonly Compilation _compilation;

        public DbContextAnalyzer(Compilation compilation)
        {
            _compilation = compilation;
        }

        public bool CanAnalyze(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDeclaration &&
                   IsDbContextClass(classDeclaration);
        }

        public DbContextMetadata Analyze(SyntaxNode node)
        {
            var classDeclaration = (ClassDeclarationSyntax)node;
            var semanticModel = _compilation.GetSemanticModel(classDeclaration.SyntaxTree);

            var contextName = classDeclaration.Identifier.Text;
            var connectionStringName = contextName.Replace("Context", "Connection");

            var dbContext = new DbContextMetadata
            {
                Name = contextName,
                IsPartial = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
                IsPublic = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)),
                ConnectionStringName = connectionStringName,
                Provider = ExtractDatabaseProvider(classDeclaration),
                DbSets = new List<DbSetMetadata>(),
                Attributes = ExtractAttributes(classDeclaration.AttributeLists)
            };

            foreach (var member in classDeclaration.Members)
            {
                if (member is PropertyDeclarationSyntax property && IsDbSetProperty(property))
                {
                    var dbSet = AnalyzeDbSetProperty(property, semanticModel);
                    dbContext.DbSets.Add(dbSet);
                }
            }

            return dbContext;
        }

        private bool IsDbContextClass(ClassDeclarationSyntax classDeclaration)
        {
            // Check if it inherits from DbContext
            if (classDeclaration.BaseList != null)
            {
                foreach (var baseType in classDeclaration.BaseList.Types)
                {
                    var baseTypeName = baseType.ToString();
                    if (baseTypeName == "DbContext" || baseTypeName.EndsWith(".DbContext"))
                        return true;
                }
            }

            // Or has a name ending with "Context"
            return classDeclaration.Identifier.Text.EndsWith("Context");
        }

        private bool IsDbSetProperty(PropertyDeclarationSyntax property)
        {
            var typeName = property.Type.ToString();
            return typeName.StartsWith("DbSet<") || typeName.Contains(".DbSet<");
        }

        private DbSetMetadata AnalyzeDbSetProperty(PropertyDeclarationSyntax property, SemanticModel semanticModel)
        {
            var typeName = property.Type.ToString();
            var entityTypeName = ExtractEntityTypeName(typeName);

            return new DbSetMetadata
            {
                Name = property.Identifier.Text,
                EntityTypeName = entityTypeName
            };
        }

        private string ExtractEntityTypeName(string dbSetTypeName)
        {
            // Extract T from DbSet<T>
            var startIndex = dbSetTypeName.IndexOf('<') + 1;
            var endIndex = dbSetTypeName.LastIndexOf('>');

            if (startIndex > 0 && endIndex > startIndex)
            {
                return dbSetTypeName.Substring(startIndex, endIndex - startIndex).Trim();
            }

            return string.Empty;
        }

        private DatabaseProvider ExtractDatabaseProvider(ClassDeclarationSyntax classDeclaration)
        {
            // Look for [DbProvider] attribute
            foreach (var attributeList in classDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name.ToString();
                    if (attributeName == "DbProvider" || attributeName == "DbProviderAttribute")
                    {
                        if (attribute.ArgumentList != null && attribute.ArgumentList.Arguments.Count > 0)
                        {
                            var argument = attribute.ArgumentList.Arguments[0].ToString();
                            if (argument.Contains("SqlServer"))
                                return DatabaseProvider.SqlServer;
                            if (argument.Contains("Oracle"))
                                return DatabaseProvider.Oracle;
                            if (argument.Contains("PostgreSql"))
                                return DatabaseProvider.PostgreSql;
                            if (argument.Contains("MySql"))
                                return DatabaseProvider.MySql;
                            if (argument.Contains("Sqlite"))
                                return DatabaseProvider.Sqlite;
                        }
                    }
                }
            }

            // Default to SQL Server if not specified
            return DatabaseProvider.SqlServer;
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
    }
}