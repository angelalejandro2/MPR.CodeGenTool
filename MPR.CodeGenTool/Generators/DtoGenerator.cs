using Scriban;
using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MPR.CodeGenTool.Models;

namespace MPR.CodeGenTool.Generators
{
    public class DtoGenerator
    {
        public static void GenerateQueryDtosFromDbContexts(string infraAssemblyPath, string solutionName, string outputPath)
        {
            var assembly = Assembly.LoadFrom(infraAssemblyPath);

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

                    var entityModel = new EntityModel
                    {
                        Name = entityType.Name,
                        Properties = entityType
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Select(p => new PropertyModel
                            {
                                Name = p.Name,
                                Type = GetFriendlyTypeName(p.PropertyType)
                            }).ToList(),
                        PrimaryKeys = new List<string>() // Se puede completar más adelante desde YAML o metadata extendida
                    };

                    GenerateQueryDto(entityModel, solutionName, outputPath);
                }
            }
        }

        private static void GenerateQueryDto(EntityModel entity, string solutionName, string outputPath)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "Templates", "Dtos", "Queries", "Dto.scriban");

            var templateText = File.ReadAllText(templatePath);
            var scribanTemplate = Template.Parse(templateText);

            var result = scribanTemplate.Render(new
            {
                solutionName,
                entity = new
                {
                    name = entity.Name,
                    properties = entity.Properties.Select(p => new { name = p.Name, type = p.Type }).ToList(),
                    primaryKeys = entity.PrimaryKeys
                }
            }, member => member.Name);

            var outputFile = Path.Combine(outputPath, $"{entity.Name}Dto.cs");
            Directory.CreateDirectory(outputPath);
            File.WriteAllText(outputFile, result);
            Console.WriteLine($"✅ DTO generado: {outputFile}");
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