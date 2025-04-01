using Scriban;
using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using MPR.CodeGenTool.Models;
using System.Collections.Generic;

namespace MPR.CodeGenTool.Generators
{
    public class UnitOfWorkInterfaceGenerator
    {
        public static void GenerateInterface(string infraAssemblyPath, string solutionName, string outputPath)
        {
            var assembly = Assembly.LoadFrom(infraAssemblyPath);

            var dbContextTypes = assembly.GetTypes()
                .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            var repositories = new List<string>();

            foreach (var dbContextType in dbContextTypes)
            {
                var dbSetProps = dbContextType.GetProperties()
                    .Where(p => p.PropertyType.IsGenericType &&
                                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .ToList();

                foreach (var prop in dbSetProps)
                {
                    var entityType = prop.PropertyType.GetGenericArguments()[0];
                    repositories.Add(entityType.Name);
                }
            }

            GenerateIUnitOfWorkInterface(repositories, solutionName, outputPath);
        }

        private static void GenerateIUnitOfWorkInterface(List<string> repositories, string solutionName, string outputPath)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "Templates", "Domain", "UnitOfWork", "IUnitOfWork.scriban");
            var templateText = File.ReadAllText(templatePath);
            var scribanTemplate = Template.Parse(templateText);

            var result = scribanTemplate.Render(new
            {
                solutionName,
                repositories
            }, member => member.Name);

            var filePath = Path.Combine(outputPath, "IUnitOfWork.cs");
            Directory.CreateDirectory(outputPath);
            File.WriteAllText(filePath, result);
            Console.WriteLine($"✅ Interface IUnitOfWork generada: {filePath}");
        }
    }
}