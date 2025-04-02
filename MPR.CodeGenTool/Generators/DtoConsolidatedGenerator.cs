using Scriban;
using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using MPR.CodeGenTool.Models;
using MPR.CodeGenTool.Helpers;

namespace MPR.CodeGenTool.Generators
{
    public class DtoConsolidatedGenerator
    {
        public static void Generate(string infraAssemblyPath, string solutionName, string outputPath)
        {
            var assembly = Assembly.LoadFrom(infraAssemblyPath);
            var primaryKeysByEntity = EfCoreMetadataHelper.GetPrimaryKeysFromContexts(infraAssemblyPath);

            var dbContextTypes = assembly.GetTypes()
                .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            foreach (var dbContextType in dbContextTypes)
            {
                var dbSetProps = dbContextType.GetProperties()
                    .Where(p => p.PropertyType.IsGenericType &&
                                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .ToList();

                foreach (var prop in dbSetProps)
                {
                    var entityType = prop.PropertyType.GetGenericArguments()[0];
                    var entityName = entityType.Name;

                    var entityProps = entityType.GetProperties()
                        .Where(p => p.PropertyType.Namespace != "System.Collections.Generic")
                        .ToList();

                    var primaryKeys = primaryKeysByEntity.ContainsKey(entityName)
                        ? primaryKeysByEntity[entityName]
                        : new List<PropertyModel>();

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

                    GenerateDtos(entityModel, solutionName, outputPath);
                }
            }
        }

        private static void GenerateDtos(EntityModel entity, string solutionName, string outputPath)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "Templates", "Application", "Dtos", "DtosConsolidated.scriban");
            var template = Template.Parse(File.ReadAllText(templatePath));

            var model = new
            {
                solutionName,
                entity = new
                {
                    name = entity.Name,
                    properties = entity.Properties.Select(p => new { name = p.Name, type = p.Type }).ToList(),
                    primaryKeys = entity.PrimaryKeys.Select(k => new { name = k.Name, type = k.Type }).ToList()
                }
            };

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