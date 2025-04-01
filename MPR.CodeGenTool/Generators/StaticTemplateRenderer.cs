using Scriban;
using System;
using System.IO;

namespace MPR.CodeGenTool.Generators
{
    public static class StaticTemplateRenderer
    {
        public static void RenderTemplate(string templatePath, object model, string outputFile)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var fullTemplatePath = Path.Combine(baseDir, templatePath);

            if (!File.Exists(fullTemplatePath))
            {
                Console.WriteLine($"❌ Template no encontrado: {fullTemplatePath}");
                return;
            }

            var templateText = File.ReadAllText(fullTemplatePath);
            var scribanTemplate = Template.Parse(templateText);
            var result = scribanTemplate.Render(model, member => member.Name);

            var outputDir = Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllText(outputFile, result);
            Console.WriteLine($"✅ Archivo generado desde template: {outputFile}");
        }
    }
}