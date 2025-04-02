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
    public class CommandConsolidatedGenerator
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

                    if (!primaryKeysByEntity.TryGetValue(entityName, out var primaryKeys) || primaryKeys.Count == 0)
                    {
                        Console.WriteLine($"⚠️  Skipping {entityName} — no primary key defined.");
                        continue;
                    }

                    var entityProps = entityType.GetProperties()
                        .Where(p => p.PropertyType.Namespace != "System.Collections.Generic")
                        .ToList();

                    var entityModel = new EntityModel
                    {
                        Name = entityName,
                        Properties = entityProps.Select(p => new PropertyModel
                        {
                            Name = p.Name,
                            Type = EfCoreMetadataHelper.GetFriendlyTypeName(p.PropertyType)
                        }).ToList(),
                        PrimaryKeys = primaryKeys
                    };

                    GenerateCommands(entityModel, solutionName, outputPath);
                }
            }
        }

        private static void GenerateCommands(EntityModel entity, string solutionName, string outputPath)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "Templates", "Application", "Commands", "CommandsConsolidated.scriban");
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

            var dir = Path.Combine(outputPath, "Commands");
            Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, $"{entity.Name}Commands.cs");
            File.WriteAllText(filePath, template.Render(model, member => member.Name));

            Console.WriteLine($"✅ Commands consolidados generados para: {entity.Name}");
        }
    }
}