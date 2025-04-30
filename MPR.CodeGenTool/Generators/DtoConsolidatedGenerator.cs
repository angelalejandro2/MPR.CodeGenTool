using Scriban;
using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using MPR.CodeGenTool.Models;
using MPR.CodeGenTool.Helpers;
using MPR.CodeGenTool.Services.Metadata;

namespace MPR.CodeGenTool.Generators
{
    public class DtoConsolidatedGenerator
    {
        public static void Generate(string infraAssemblyPath, string solutionName, string outputPath)
        {
            var metadataService = new DbContextMetadataService();
            var metadataList = metadataService.LoadMetadataFromAssembly(infraAssemblyPath);

            var primaryKeysByEntity = metadataList
                .GroupBy(e => e.EntityName)
                .ToDictionary(g => g.Key, g => g.First().PrimaryKeyProperties
                    .Select(pk => new PropertyModel
                    {
                        Name = pk.Name,
                        Type = GetFriendlyTypeName(pk.Type)
                    }).ToList());

            foreach (var entity in metadataList)
            {
                var props = entity.ClrType.GetProperties()
                    .Where(p => p.PropertyType.Namespace != "System.Collections.Generic")
                    .ToList();

                var entityModel = new EntityModel
                {
                    Name = entity.EntityName,
                    Properties = props.Select(p => new PropertyModel
                    {
                        Name = p.Name,
                        Type = GetFriendlyTypeName(p.PropertyType)
                    }).ToList(),
                    PrimaryKeys = primaryKeysByEntity.ContainsKey(entity.EntityName)
                        ? primaryKeysByEntity[entity.EntityName]
                        : new List<PropertyModel>()
                };

                GenerateDtos(entityModel, solutionName, outputPath);
            }
        }

        private static void GenerateDtos(EntityModel entity, string solutionName, string outputPath)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "Templates", "Application", "Dtos", "DtosConsolidated.scriban");
            var template = Template.Parse(File.ReadAllText(templatePath));

            var primaryKeyNames = entity.PrimaryKeys.Select(k => k.Name).ToHashSet();

            var model = new
            {
                solutionName,
                entity = new
                {
                    name = entity.Name,
                    primaryKeys = entity.PrimaryKeys.Select(k => new { name = k.Name, type = k.Type }).ToList(),
                    properties = entity.Properties
                        .Where(p => !primaryKeyNames.Contains(p.Name)) // 🚫 Elimina duplicados aquí
                        .Select(p => new { name = p.Name, type = p.Type }).ToList()
                }
            };

            if (!entity.PrimaryKeys.Any())
            {
                Console.WriteLine($"⚠️  {entity.Name} no tiene claves primarias. Solo se generará CreateModel.");
            }
            var dir = Path.Combine(outputPath, "Dtos");
            Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, $"{entity.Name}Dtos.cs");
            File.WriteAllText(filePath, template.Render(model, member => member.Name));

            Console.WriteLine($"✅ DTOs consolidados generados para: {entity.Name}");
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