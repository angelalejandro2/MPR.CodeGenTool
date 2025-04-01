using Scriban;
using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MPR.CodeGenTool.Models;

namespace MPR.CodeGenTool.Generators
{
    public class RepositoryInterfaceGenerator
    {
        public static void GenerateEntityRepositoryInterfaces(string infraAssemblyPath, string solutionName, string outputPath)
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
                        Name = entityType.Name
                    };

                    GenerateRepositoryInterface(entityModel, solutionName, outputPath);
                }
            }
        }

        private static void GenerateRepositoryInterface(EntityModel entity, string solutionName, string outputPath)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "Templates", "Domain", "Repositories", "IEntityRepository.scriban");
            var templateText = File.ReadAllText(templatePath);
            var scribanTemplate = Template.Parse(templateText);

            var result = scribanTemplate.Render(new
            {
                solutionName,
                entity = new
                {
                    name = entity.Name
                }
            }, member => member.Name);

            var filePath = Path.Combine(outputPath, $"I{entity.Name}Repository.cs");
            Directory.CreateDirectory(outputPath);
            File.WriteAllText(filePath, result);
            Console.WriteLine($"✅ Interface generada: {filePath}");
        }
    }
}