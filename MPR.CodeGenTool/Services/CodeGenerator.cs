using System;
using System.IO;
using MPR.CodeGenTool.Generators;

namespace MPR.CodeGenTool.Services
{
    public static class CodeGenerator
    {
        public static void GenerateFromContexts(string solutionName)
        {
            Console.WriteLine($"🚀 Generando código para solución: {solutionName}");

            // Ruta esperada del DLL de Infrastructure compilado
            var dllPath = Path.Combine("MPR.SampleProject", $"{solutionName}.Infrastructure", "bin", "Debug", "net9.0", $"{solutionName}.Infrastructure.dll");
            var outputPath = Path.Combine("MPR.SampleProject", $"{solutionName}.Application", "Dtos", "Queries");

            if (!File.Exists(dllPath))
            {
                Console.WriteLine($"❌ No se encontró el assembly en: {dllPath}");
                Console.WriteLine("Asegúrate de compilar el proyecto Infrastructure antes de ejecutar generate.");
                return;
            }

            // Llamamos al generador de DTOs
            DtoGenerator.GenerateQueryDtosFromDbContexts(dllPath, solutionName, outputPath);

            var repoOutput = Path.Combine("MPR.SampleProject", $"{solutionName}.Domain", "Repositories");
            RepositoryInterfaceGenerator.GenerateEntityRepositoryInterfaces(dllPath, solutionName, repoOutput);

            var uowOutput = Path.Combine("MPR.SampleProject", $"{solutionName}.Domain", "UnitOfWork");
            UnitOfWorkInterfaceGenerator.GenerateInterface(dllPath, solutionName, uowOutput);

            var repoImplOutput = Path.Combine("MPR.SampleProject", $"{solutionName}.Infrastructure", "Repositories");
            RepositoryImplementationGenerator.GenerateEntityRepositories(dllPath, solutionName, repoImplOutput);

            var uowImplOutput = Path.Combine("MPR.SampleProject", $"{solutionName}.Infrastructure", "UnitOfWork");
            UnitOfWorkImplementationGenerator.GenerateImplementation(dllPath, solutionName, uowImplOutput);
        }
    }
}
