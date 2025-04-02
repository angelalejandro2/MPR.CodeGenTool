using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MPR.CodeGenTool.Models;

namespace MPR.CodeGenTool.Helpers
{
    public static class EfCoreMetadataHelper
    {
        public static Dictionary<string, List<PropertyModel>> GetPrimaryKeysFromContexts(string infraAssemblyPath)
        {
            var result = new Dictionary<string, List<PropertyModel>>();
            var assembly = Assembly.LoadFrom(infraAssemblyPath);

            var dbContextTypes = assembly.GetTypes()
                .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            foreach (var dbContextType in dbContextTypes)
            {
                Console.WriteLine($"\n🔍 Analyzing DbContext: {dbContextType.Name}");

                try
                {
                    var optionsType = typeof(DbContextOptions<>).MakeGenericType(dbContextType);
                    var optionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(dbContextType);
                    var optionsBuilder = Activator.CreateInstance(optionsBuilderType)!;

                    var useInMemoryMethod = optionsBuilderType.GetMethod("UseInMemoryDatabase", new[] { typeof(string) });
                    useInMemoryMethod?.Invoke(optionsBuilder, new object[] { $"FakeDb_{Guid.NewGuid()}" });

                    // ✅ Usamos BindingFlags para evitar ambigüedad
                    var optionsProperty = optionsBuilderType
                        .GetProperty("Options", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    var options = optionsProperty?.GetValue(optionsBuilder);

                    var contextInstance = Activator.CreateInstance(dbContextType, options) as DbContext;
                    if (contextInstance == null)
                    {
                        Console.WriteLine("⛔ Could not create context instance.");
                        continue;
                    }

                    var model = contextInstance.Model;
                    foreach (var entityType in model.GetEntityTypes())
                    {
                        Console.WriteLine($"➡️  Entity: {entityType.ClrType.Name}");

                        var primaryKey = entityType.FindPrimaryKey();
                        if (primaryKey == null)
                        {
                            Console.WriteLine($"⛔ No primary key found for {entityType.ClrType.Name}");
                            continue;
                        }

                        var keyModels = primaryKey.Properties.Select(p => new PropertyModel
                        {
                            Name = p.Name,
                            Type = GetFriendlyTypeName(p.ClrType)
                        }).ToList();

                        Console.WriteLine($"✅ Primary keys: {string.Join(", ", keyModels.Select(k => k.Name))}");
                        result[entityType.ClrType.Name] = keyModels;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"🔥 Error processing {dbContextType.Name}: {ex.Message}");
                }
            }

            return result;
        }

        public static string GetFriendlyTypeName(Type type)
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