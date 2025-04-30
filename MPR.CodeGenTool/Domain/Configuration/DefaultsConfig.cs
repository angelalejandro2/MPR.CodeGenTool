using System.Collections.Generic;

namespace MPR.CodeGenTool.Domain.Configuration
{
    /// <summary>
    /// Default configuration for all entities
    /// </summary>
    public class DefaultsConfig
    {
        public MethodsConfig GenerateMethods { get; set; } = new();
        public Dictionary<string, string> DefaultPolicies { get; set; } = new();
    }
}