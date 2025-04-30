using Scriban;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MPR.CodeGenTool.Models;
using MPR.CodeGenTool.Services.Metadata;

namespace MPR.CodeGenTool.Generators
{
    public class QueryHandlerConsolidatedGenerator
    {
        public static void Generate(string infraAssemblyPath, string solutionName, string outputPath)
        {
            var metadataService = new DbContextMetadataService();
            var metadataList = metadataService.LoadMetadataFromAssembly(infraAssemblyPath);

            foreach (var entity in metadataList)
            {
                var entityName = entity.EntityName;
                var clrType = entity.ClrType;

                var entityProps = clrType.GetProperties()
                    .Where(p => p.PropertyType.Namespace != "System.Collections.Generic")
                    .ToList();

                var primaryKeys = entity.PrimaryKeyProperties.Select(pk => new PropertyModel
                {
                    Name = pk.Name,
                    Type = GetFriendlyTypeName(pk.Type)
                }).ToList();

                var entityModel = new EntityModel
                {
                    Name = entityName,
                    Properties = entityProps.Select(p => new PropertyModel
                    {
                        Name = p.Name,
                        Type = GetFriendlyTypeName(p.PropertyType)
                    }).ToList(),
                    PrimaryKeys = primaryKeys
                };

                GenerateQueryHandlers(entityModel, solutionName, outputPath);
            }
        }

        private static void GenerateQueryHandlers(EntityModel entity, string solutionName, string outputPath)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "Templates", "Application", "QueryHandlers", "QueryHandlersConsolidated.scriban");
            var template = Template.Parse(File.ReadAllText(templatePath));

            if (template.HasErrors)
            {
                Console.WriteLine("❌ Error en el template Scriban:");
                foreach (var message in template.Messages)
                {
                    Console.WriteLine($"- {message}");
                }
                return;
            }

            var primaryKeyNames = entity.PrimaryKeys.Select(k => k.Name).ToHashSet();

            var model = new
            {
                solutionName,
                entity = new
                {
                    name = entity.Name,
                    primaryKeys = entity.PrimaryKeys.Select(k => new { name = k.Name, type = k.Type }).ToList(),
                    properties = entity.Properties
                        .Where(p => !primaryKeyNames.Contains(p.Name))
                        .Select(p => new { name = p.Name, type = p.Type }).ToList()
                }
            };

            if (!entity.PrimaryKeys.Any())
            {
                Console.WriteLine($"⚠️  {entity.Name} no tiene claves primarias. Se omite generación de comandos o handlers.");
                return;
            }

            var dir = Path.Combine(outputPath, "QueryHandlers");
            Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, $"{entity.Name}QueryHandlers.cs");
            File.WriteAllText(filePath, template.Render(model, member => member.Name));

            Console.WriteLine($"✅ Manejadores de consultas CQRS generados para: {entity.Name}");
        }

        private static string GetFriendlyTypeName(Type type)
        {
            if (type == typeof(int)) return "int";
            if (type == typeof(string)) return "string";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(Guid)) return "Guid";
            if (type == typeof(DateTime)) return "DateTime";
            if (type == typeof(double)) return "double";
            if (type == typeof(decimal)) return "decimal";
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return GetFriendlyTypeName(type.GetGenericArguments()[0]) + "?";
            return type.Name;
        }
    }
}