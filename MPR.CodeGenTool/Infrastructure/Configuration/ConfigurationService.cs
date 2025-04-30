// Infrastructure/Configuration/ConfigurationService.cs
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MPR.CodeGenTool.Domain.Configuration;
using MPR.CodeGenTool.Domain.Metadata;

namespace MPR.CodeGenTool.Infrastructure.Configuration
{
    public class ConfigurationService
    {
        private readonly JsonSerializerOptions _options;
        
        public ConfigurationService()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }
        
        public async Task<CodeGenerationConfig> LoadConfigAsync(string configPath)
        {
            if (!File.Exists(configPath))
            {
                // Return default configuration
                return new CodeGenerationConfig();
            }
            
            var json = await File.ReadAllTextAsync(configPath);
            return JsonSerializer.Deserialize<CodeGenerationConfig>(json, _options);
        }
        
        public async Task SaveConfigAsync(CodeGenerationConfig config, string configPath)
        {
            var json = JsonSerializer.Serialize(config, _options);
            await File.WriteAllTextAsync(configPath, json);
        }
        
        public async Task<SolutionMetadata> LoadMetadataAsync(string metadataPath)
        {
            if (!File.Exists(metadataPath))
            {
                throw new FileNotFoundException($"Metadata file not found: {metadataPath}");
            }
            
            var json = await File.ReadAllTextAsync(metadataPath);
            return JsonSerializer.Deserialize<SolutionMetadata>(json, _options);
        }
        
        public async Task SaveMetadataAsync(SolutionMetadata metadata, string metadataPath)
        {
            var directory = Path.GetDirectoryName(metadataPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var json = JsonSerializer.Serialize(metadata, _options);
            await File.WriteAllTextAsync(metadataPath, json);
        }
    }
}