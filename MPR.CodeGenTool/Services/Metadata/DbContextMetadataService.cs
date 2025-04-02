using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MPR.CodeGenTool.Models;

namespace MPR.CodeGenTool.Services.Metadata;

public class DbContextMetadataService : IDbContextMetadataService
{
    public List<EntityMetadata> LoadMetadataFromAssembly(string assemblyPath)
    {
        var assembly = Assembly.LoadFrom(assemblyPath);

        var dbContextTypes = assembly
            .GetTypes()
            .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

        var result = new List<EntityMetadata>();

        foreach (var contextType in dbContextTypes)
        {
            var sqliteConnection = new SqliteConnection("Data Source=:memory:");
            sqliteConnection.Open();

            // Creamos el DbContextOptionsBuilder<T>
            var builderGenericType = typeof(DbContextOptionsBuilder<>).MakeGenericType(contextType);
            dynamic builderGeneric = Activator.CreateInstance(builderGenericType)!;

            DbContextOptionsBuilder builder = builderGeneric;
            builder.UseSqlite(sqliteConnection);

            var optionsProperty = builderGenericType.GetProperty("Options", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)!;
            var options = optionsProperty.GetValue(builderGeneric)!;

            var context = (DbContext)Activator.CreateInstance(contextType, options)!;

            var model = context.Model;

            foreach (var entityType in model.GetEntityTypes())
            {
                var primaryKey = entityType.FindPrimaryKey();

                var entityMetadata = new EntityMetadata
                {
                    ContextName = contextType.Name,
                    EntityName = entityType.ClrType.Name,
                    ClrType = entityType.ClrType,
                    PrimaryKeyProperties = primaryKey?.Properties.Select(p => new KeyProperty
                    {
                        Name = p.Name,
                        Type = p.ClrType,
                        IsNullable = p.IsNullable
                    }).ToList() ?? new()
                };

                result.Add(entityMetadata);
            }

            sqliteConnection.Close();
        }

        return result;
    }
}