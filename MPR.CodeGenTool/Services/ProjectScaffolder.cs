using System.Diagnostics;
using System;
using MPR.CodeGenTool.Generators;

namespace MPR.CodeGenTool.Services
{
    public static class ProjectScaffolder
    {
        public static void CreateSolution(string baseName)
        {
            // Lógica para crear la solución base.
            // Esta es solo una estructura vacía por ahora.
            Console.WriteLine($"🛠️ Creando solución base para: {baseName}");

            Console.WriteLine("== Creando solución base para API REST ==");

            // 1. Nombre base
            var solutionName = $"{baseName}";
            var root = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), solutionName));
            var solutionPath = Path.Combine(root, solutionName);
            var domainPath = Path.Combine(solutionPath + ".Domain");
            var infraPath = Path.Combine(solutionPath + ".Infrastructure");
            Directory.CreateDirectory(root);

            // 2. Crear solución
            Run("dotnet", $"new sln -n {solutionName}", root);

            // 3. Capas/proyectos
            var layers = new[] { "Api", "Application", "Domain", "Infrastructure", "IntegrationTests" };

            foreach (var layer in layers)
            {
                var projectName = $"{solutionName}.{layer}";
                var projectDir = Path.Combine(root, projectName);
                Directory.CreateDirectory(projectDir);

                var template = layer switch
                {
                    "Api" => "webapi",
                    "IntegrationTests" => "xunit",
                    _ => "classlib"
                };

                // 🔧 No usar -n para evitar subcarpetas duplicadas
                Run("dotnet", $"new {template}", projectDir);

                // 🔥 Eliminar archivos por defecto generados
                if (template == "classlib")
                {
                    var defaultFile = Path.Combine(projectDir, "Class1.cs");
                    if (File.Exists(defaultFile))
                    {
                        File.Delete(defaultFile);
                        Console.WriteLine($"🧹 Eliminado: {defaultFile}");
                    }
                }
                else if (template == "xunit")
                {
                    var testFile = Path.Combine(projectDir, "UnitTest1.cs");
                    if (File.Exists(testFile))
                    {
                        File.Delete(testFile);
                        Console.WriteLine($"🧹 Eliminado: {testFile}");
                    }
                }

                var csprojPath = Path.Combine(projectDir, $"{projectName}.csproj");
                Run("dotnet", $"sln \"{Path.Combine(root, $"{solutionName}.sln")}\" add \"{csprojPath}\"");

                CreateProjectStructure(layer, projectDir);
            }

            // Crear subcarpetas dentro de cada proyecto
            void CreateProjectStructure(string layer, string projectDir)
            {
                void Mk(params string[] paths)
                {
                    foreach (var path in paths)
                        Directory.CreateDirectory(Path.Combine(projectDir, path));
                }

                switch (layer)
                {
                    case "Application":
                        Mk("Commands", "Queries", "Dtos/Commands", "Dtos/Queries", "Interfaces", "Mappings");
                        break;
                    case "Domain":
                        Mk("Entities", "Interfaces/Repositories", "Common");
                        break;
                    case "Infrastructure":
                        Mk("Context", "Repositories");
                        break;
                    case "Api":
                        Mk("Controllers/V1", "Middlewares", "Extensions");
                        break;
                    case "IntegrationTests":
                        Mk("Controllers/V1", "Fixtures");
                        break;
                }
            }

 
            // 4. Agregar referencias entre proyectos
            string Proj(string layer) =>
                Path.Combine(root, $"{solutionName}.{layer}", $"{solutionName}.{layer}.csproj");

            // 🔄 NUEVO orden de referencias
            Run("dotnet", $"add \"{Proj("Api")}\" reference \"{Proj("Application")}\"");
            Run("dotnet", $"add \"{Proj("Application")}\" reference \"{Proj("Domain")}\"");
            Run("dotnet", $"add \"{Proj("Application")}\" reference \"{Proj("Infrastructure")}\"");
            Run("dotnet", $"add \"{Proj("Infrastructure")}\" reference \"{Proj("Domain")}\"");
            Run("dotnet", $"add \"{Proj("IntegrationTests")}\" reference \"{Proj("Api")}\"");
            Run("dotnet", $"add \"{Proj("IntegrationTests")}\" reference \"{Proj("Application")}\"");
            Run("dotnet", $"add \"{Proj("IntegrationTests")}\" reference \"{Proj("Infrastructure")}\"");

            // NuGet packages por proyecto
            var packageMap = new Dictionary<string, string[]>
            {
                ["Application"] =
                [
                    "AutoMapper",
        "MediatR",
        "FluentValidation"
                ],
                ["Infrastructure"] =
                [
                    "Microsoft.EntityFrameworkCore",
        "Microsoft.EntityFrameworkCore.Design",
        "Microsoft.EntityFrameworkCore.Tools",
        "Microsoft.EntityFrameworkCore.SqlServer",
        "Oracle.EntityFrameworkCore"
                ],
                ["Api"] =
                [
                    "Swashbuckle.AspNetCore",
        "Asp.Versioning.Mvc",
        "Asp.Versioning.Mvc.ApiExplorer",
        "Microsoft.AspNetCore.Mvc.NewtonsoftJson",
    ],
                ["IntegrationTests"] =
                [
                    "xunit",
        "FluentAssertions",
        "Microsoft.AspNetCore.Mvc.Testing",
        "Moq"
                ],
            };

            foreach (var entry in packageMap)
            {
                var layer = entry.Key;
                var project = $"{solutionName}.{layer}";
                var projectDir = Path.Combine(root, project);
                foreach (var pkg in entry.Value)
                {
                    Run("dotnet", $"add {project}.csproj package {pkg}", projectDir);
                }
            }

            // 🔽 Crear YAML de configuración global
            var yamlGlobal = $"""
solutionName: {solutionName}
versioning:
  default: v1
  type: urlSegment
naming:
  useDtoSuffix: true
defaults:
  readOnly: false
  generateController: true
  generateCommands: true
  generateQueries: true
""";

            File.WriteAllText(Path.Combine(root, "mpr.codegen.yaml"), yamlGlobal);
            Console.WriteLine("📝 Generado: mpr.codegen.yaml");

            // 🔽 Crear YAML inicial por contexto (dummy con nombre base)
            var infrastructurePath = Path.Combine(root, $"{solutionName}.Infrastructure");
            var infraYaml = $"""
contexts:
  - name: {baseName}DbContext
    provider: SqlServer
    version: v1
    entities:
      - name: ExampleEntity
        readOnly: true
""";

            File.WriteAllText(Path.Combine(infrastructurePath, "mpr.codegen.infrastructure.yaml"), infraYaml);
            Console.WriteLine("📝 Generado: mpr.codegen.infrastructure.yaml");

            // 🔽 Crear YAML para Application
            var appPath = Path.Combine(root, $"{solutionName}.Application");
            var yamlApp = $"""
naming:
  dtoSuffix: Dto
  commandSuffix: Command
  querySuffix: Query
validations:
  generateFluentValidators: true
""";
            File.WriteAllText(Path.Combine(appPath, "mpr.codegen.application.yaml"), yamlApp);
            Console.WriteLine("📝 Generado: mpr.codegen.application.yaml");

            // 🔽 Crear YAML para IntegrationTests
            var testsPath = Path.Combine(root, $"{solutionName}.IntegrationTests");
            var yamlTests = $"""
defaults:
  generateTestsFor: [get, post, put, delete]
  versionedControllers: true
""";
            File.WriteAllText(Path.Combine(testsPath, "mpr.codegen.tests.yaml"), yamlTests);
            Console.WriteLine("📝 Generado: mpr.codegen.tests.yaml");

            StaticTemplateRenderer.RenderTemplate(
    "Templates/Domain/Repositories/IGenericRepository.scriban",
    new { solutionName },
    Path.Combine(domainPath, "Repositories", "IGenericRepository.cs")
);

StaticTemplateRenderer.RenderTemplate(
    "Templates/Infrastructure/Repositories/GenericRepository.scriban",
    new { solutionName },
    Path.Combine(infraPath, "Repositories", "GenericRepository.cs")
);

            Console.WriteLine("== ✅ Solución creada exitosamente ==");

            // Ejecutar comandos
            void Run(string file, string args, string? workingDir = null)
            {
                Console.WriteLine($"> {file} {args}");
                var psi = new ProcessStartInfo
                {
                    FileName = file,
                    Arguments = args,
                    WorkingDirectory = workingDir ?? Directory.GetCurrentDirectory(),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                using var proc = Process.Start(psi);
                proc!.WaitForExit();
                Console.WriteLine(proc.StandardOutput.ReadToEnd());
                Console.Error.WriteLine(proc.StandardError.ReadToEnd());
            }

        }
    }
}