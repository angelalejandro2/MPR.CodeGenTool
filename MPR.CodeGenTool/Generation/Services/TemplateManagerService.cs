// Generation/Services/TemplateManagerService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Scriban;
using Scriban.Runtime;

namespace MPR.CodeGenTool.Generation.Services
{
    public class TemplateManagerService
    {
        private readonly Dictionary<string, Template> _cachedTemplates = new();
        private readonly string _templateBasePath;
        
        public TemplateManagerService(string templateBasePath = null)
        {
            _templateBasePath = templateBasePath ?? 
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Templates");
        }
        
        public async Task<Template> GetTemplateAsync(string templatePath)
        {
            if (_cachedTemplates.TryGetValue(templatePath, out var cachedTemplate))
            {
                return cachedTemplate;
            }
            
            string fullPath = Path.IsPathRooted(templatePath) ? 
                templatePath : Path.Combine(_templateBasePath, templatePath);
            
            if (!File.Exists(fullPath))
            {
                // Try to find it as an embedded resource
                var template = await LoadEmbeddedTemplateAsync(templatePath);
                if (template != null)
                {
                    _cachedTemplates[templatePath] = template;
                    return template;
                }
                
                throw new FileNotFoundException($"Template not found: {templatePath}");
            }
            
            string templateContent = await File.ReadAllTextAsync(fullPath);
            var parsedTemplate = Template.Parse(templateContent);
            
            if (parsedTemplate.HasErrors)
            {
                throw new Exception($"Error parsing template {templatePath}: {parsedTemplate.Messages[0]}");
            }
            
            _cachedTemplates[templatePath] = parsedTemplate;
            return parsedTemplate;
        }
        
        private async Task<Template> LoadEmbeddedTemplateAsync(string templatePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"MPR.CodeGenTool.Resources.Templates.{templatePath.Replace('/', '.')}";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                return null;
            
            using var reader = new StreamReader(stream);
            string templateContent = await reader.ReadToEndAsync();
            
            var parsedTemplate = Template.Parse(templateContent);
            if (parsedTemplate.HasErrors)
            {
                throw new Exception($"Error parsing embedded template {templatePath}: {parsedTemplate.Messages[0]}");
            }
            
            return parsedTemplate;
        }
        
        public async Task<string> RenderTemplateAsync<T>(string templatePath, T model)
        {
            var template = await GetTemplateAsync(templatePath);
            
            var scriptObject = new ScriptObject();
            scriptObject.Import(model);
            
            var context = new TemplateContext();
            context.PushGlobal(scriptObject);
            
            // Add custom functions
            scriptObject.Import("string", new StringFunctions());
            
            return await template.RenderAsync(context);
        }
    }
    
    public class StringFunctions
    {
        public static string PascalCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            return char.ToUpperInvariant(text[0]) + text.Substring(1);
        }
        
        public static string CamelCase(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            return char.ToLowerInvariant(text[0]) + text.Substring(1);
        }
        
        public static string Pluralize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            // Very basic pluralization - in a real app, use a library like Humanizer
            if (text.EndsWith("y"))
                return text.Substring(0, text.Length - 1) + "ies";
            if (text.EndsWith("s") || text.EndsWith("x") || text.EndsWith("z") || 
                text.EndsWith("ch") || text.EndsWith("sh"))
                return text + "es";
            
            return text + "s";
        }
    }
}