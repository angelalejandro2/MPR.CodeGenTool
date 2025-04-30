using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MPR.CodeGenTool.Services
{
    public static class ProjectStructureService
    {
        /// <summary>
        /// Inserta una o más líneas en Program.cs después de una línea clave, evitando duplicados.
        /// </summary>
        public static void InjectLinesIntoProgram(string programCsPath, string anchorLineContains, IEnumerable<string> linesToInject)
        {
            if (!File.Exists(programCsPath))
            {
                Console.WriteLine($"❌ Program.cs no encontrado en: {programCsPath}");
                return;
            }

            var lines = File.ReadAllLines(programCsPath).ToList();
            var insertIndex = lines.FindIndex(l => l.Contains(anchorLineContains));

            if (insertIndex == -1)
            {
                Console.WriteLine("⚠️ Línea clave no encontrada en Program.cs. No se insertaron líneas.");
                return;
            }

            bool anyInserted = false;

            foreach (var newLine in linesToInject.Reverse())
            {
                if (!lines.Any(l => l.Contains(newLine)))
                {
                    lines.Insert(insertIndex + 1, newLine);
                    anyInserted = true;
                }
            }

            if (anyInserted)
            {
                File.WriteAllLines(programCsPath, lines);
                Console.WriteLine("✅ Líneas insertadas correctamente en Program.cs");
            }
            else
            {
                Console.WriteLine("ℹ️ Todas las líneas ya estaban presentes en Program.cs");
            }
        }

        /// <summary>
        /// Helper para AutoMapper específicamente.
        /// </summary>
        public static void InjectAutoMapper(string programCsPath)
        {
            InjectLinesIntoProgram(
                programCsPath,
                "var builder = WebApplication.CreateBuilder",
                new[] { "builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());" }
            );
        }
    }
}