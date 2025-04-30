using System.Collections.Generic;

namespace MPR.CodeGenTool.Domain.Configuration
{
    /// <summary>
    /// Configuration for an entity
    /// </summary>
    public class EntityConfig
    {
        // Optional custom name for the entity (different from source class name)
        public string Name { get; set; }
        
        // Navigation properties to include when fetching data
        public List<string> Includes { get; set; } = new();
        
        // Custom DB to DTO property mappings
        public Dictionary<string, string> Mappings { get; set; } = new();
        
        // Control which HTTP methods to generate
        public MethodsConfig GenerateMethods { get; set; } = new();
        
        // Custom authorization policies per HTTP method
        public Dictionary<string, string> Policies { get; set; } = new();
        
        // Whether to generate service/controller for this entity
        public bool GenerateService { get; set; } = true;
        public bool GenerateControllers { get; set; } = true;
    }
}