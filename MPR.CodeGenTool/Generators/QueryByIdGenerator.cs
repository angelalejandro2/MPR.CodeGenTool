using Scriban;
using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using MPR.CodeGenTool.Models;

namespace MPR.CodeGenTool.Generators
{
    public class QueryByIdGenerator
    {
        public static void Generate(string infraAssemblyPath, string solutionName, string outputPath)
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
                    var props = entityType.GetProperties();

                    var keyProps = props.Where(p => p.Name.ToLower().Contains("id")).ToList();

                    var entityModel = new EntityModel
                    {
                        Name = entityType.Name,
                        PrimaryKeys = keyProps.Select(p => new PropertyModel
                        {
                            Name = p.Name,
                            Type = GetFriendlyTypeName(p.PropertyType)
                        }).ToList()
                    };

                    GenerateQueryAndHandler(entityModel, solutionName, outputPath);
                }
            }
        }

        private static void GenerateQueryAndHandler(EntityModel entity, string solutionName, string outputPath)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var queryPath = Path.Combine(baseDir, "Templates", "Application", "Queries", "GetByIdQuery.scriban");
            var handlerPath = Path.Combine(baseDir, "Templates", "Application", "Queries", "GetByIdQueryHandler.scriban");

            var queryTemplate = Template.Parse(File.ReadAllText(queryPath));
            var handlerTemplate = Template.Parse(File.ReadAllText(handlerPath));

            var model = new
            {
                solutionName,
                entity = new
                {
                    name = entity.Name,
                    keys = entity.PrimaryKeys.Select(k => new { name = k.Name, type = k.Type }).ToList()
                }
            };

            var queryCode = queryTemplate.Render(model, member => member.Name);
            var handlerCode = handlerTemplate.Render(model, member => member.Name);

            var dir = Path.Combine(outputPath, entity.Name, "Queries");
            Directory.CreateDirectory(dir);

            File.WriteAllText(Path.Combine(dir, $"Get{entity.Name}ByIdQuery.cs"), queryCode);
            File.WriteAllText(Path.Combine(dir, $"Get{entity.Name}ByIdQueryHandler.cs"), handlerCode);

            Console.WriteLine($"✅ Query y Handler generados para: {entity.Name}");
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