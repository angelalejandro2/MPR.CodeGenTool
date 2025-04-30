using System.Collections.Generic;

namespace MPR.CodeGenTool.Domain.Configuration
{
    /// <summary>
    /// Configuration for the code generation process
    /// </summary>
    public class CodeGenerationConfig
    {
        public string SolutionName { get; set; }
        public string RootNamespace { get; set; }
        public string OutputDirectory { get; set; }
        public DefaultsConfig Defaults { get; set; } = new();
        public Dictionary<string, EntityConfig> Entities { get; set; } = new();
        public Dictionary<string, DbContextConfig> DbContexts { get; set; } = new();
    }
}