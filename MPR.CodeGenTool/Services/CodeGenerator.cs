using System;
using System.IO;
using MPR.CodeGenTool.Generators;
using MPR.CodeGenTool.Services.Metadata;

namespace MPR.CodeGenTool.Services
{
    public static class CodeGenerator
    {
        public static void GenerateFromContexts(string solutionName)
        {
            Console.WriteLine($"🚀 Generando código para solución: {solutionName}");

            // Ruta esperada del DLL de Infrastructure compilado
            var dllPath = Path.Combine(solutionName, $"{solutionName}.Infrastructure", "bin", "Debug", "net9.0", $"{solutionName}.Infrastructure.dll");
            var outputPath = Path.Combine(solutionName, $"{solutionName}.Application", "Dtos", "Queries");

            if (!File.Exists(dllPath))
            {
                Console.WriteLine($"❌ No se encontró el assembly en: {dllPath}");
                Console.WriteLine("Asegúrate de compilar el proyecto Infrastructure antes de ejecutar generate.");
                return;
            }

            var metadataService = new DbContextMetadataService();
            var metadata = metadataService.LoadMetadataFromAssembly(dllPath);

            // foreach (var entity in metadata)
            // {
            //     Console.WriteLine($"[{entity.ContextName}] {entity.EntityName}");

            //     foreach (var pk in entity.PrimaryKeyProperties)
            //     {
            //         Console.WriteLine($"  PK: {pk.Name} ({pk.Type.Name}){(pk.IsNullable ? "?" : "")}");
            //     }
            // }

            // Llamamos al generador de DTOs
            DtoConsolidatedGenerator.Generate(dllPath, solutionName, outputPath);

            var repoOutput = Path.Combine(solutionName, $"{solutionName}.Domain", "Repositories");
            RepositoryInterfaceGenerator.GenerateEntityRepositoryInterfaces(dllPath, solutionName, repoOutput);

            var uowOutput = Path.Combine(solutionName, $"{solutionName}.Domain", "UnitOfWork");
            UnitOfWorkInterfaceGenerator.GenerateInterface(dllPath, solutionName, uowOutput);

            var repoImplOutput = Path.Combine(solutionName, $"{solutionName}.Infrastructure", "Repositories");
            RepositoryImplementationGenerator.GenerateEntityRepositories(dllPath, solutionName, repoImplOutput);

            var uowImplOutput = Path.Combine(solutionName, $"{solutionName}.Infrastructure", "UnitOfWork");
            UnitOfWorkImplementationGenerator.GenerateImplementation(dllPath, solutionName, uowImplOutput);

            var queriesOutput = Path.Combine(solutionName, $"{solutionName}.Application");
            QueryConsolidatedGenerator.Generate(dllPath, solutionName, queriesOutput);
            QueryHandlerConsolidatedGenerator.Generate(dllPath, solutionName, queriesOutput);
            CommandConsolidatedGenerator.Generate(dllPath, solutionName, queriesOutput);
            CommandHandlerConsolidatedGenerator.Generate(dllPath, solutionName, queriesOutput);
            MappingsConsolidatedGenerator.Generate(dllPath, solutionName, queriesOutput);

            var apiOutput = Path.Combine(solutionName, $"{solutionName}.Api");
            ControllerConsolidatedGenerator.Generate(dllPath, solutionName, apiOutput);

            var programPath = Path.Combine(apiOutput, "Program.cs");

            // Inyectar AutoMapper
            ProjectStructureService.InjectAutoMapper(programPath);

            // Inyectar múltiples servicios
            ProjectStructureService.InjectLinesIntoProgram(programPath,
                "var builder = WebApplication.CreateBuilder",
                new[]
                {
                    "builder.Services.AddMediatR(typeof(SomeHandler).Assembly);",
                    "builder.Services.AddSwaggerGen();"
                });
        }
    }
}
