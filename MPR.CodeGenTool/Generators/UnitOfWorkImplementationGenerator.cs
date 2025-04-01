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
    public class UnitOfWorkImplementationGenerator
    {
        public static void GenerateImplementation(string infraAssemblyPath, string solutionName, string outputPath)
        {
            var assembly = Assembly.LoadFrom(infraAssemblyPath);

            var dbContextTypes = assembly.GetTypes()
                .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            var groupedRepositories = new Dictionary<string, List<string>>();

            foreach (var dbContextType in dbContextTypes)
            {
                var contextName = dbContextType.Name;
                groupedRepositories[contextName] = new List<string>();

                var dbSetProps = dbContextType.GetProperties()
                    .Where(p => p.PropertyType.IsGenericType &&
                                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .ToList();

                foreach (var prop in dbSetProps)
                {
                    var entityType = prop.PropertyType.GetGenericArguments()[0];
                    groupedRepositories[contextName].Add(entityType.Name);
                }
            }

            GenerateUnitOfWorkClass(groupedRepositories, solutionName, outputPath);
        }

        private static void GenerateUnitOfWorkClass(Dictionary<string, List<string>> groupedRepositories, string solutionName, string outputPath)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var templatePath = Path.Combine(baseDir, "Templates", "Infrastructure", "UnitOfWork", "UnitOfWork.scriban");
            var templateText = File.ReadAllText(templatePath);
            var scribanTemplate = Template.Parse(templateText);

            var result = scribanTemplate.Render(new
            {
                solutionName,
                contexts = groupedRepositories.Select(g => new
                {
                    name = g.Key,
                    repositories = g.Value
                }).ToList()
            }, member => member.Name);

            var filePath = Path.Combine(outputPath, "UnitOfWork.cs");
            Directory.CreateDirectory(outputPath);
            File.WriteAllText(filePath, result);
            Console.WriteLine($"✅ Clase UnitOfWork generada: {filePath}");
        }
    }
}