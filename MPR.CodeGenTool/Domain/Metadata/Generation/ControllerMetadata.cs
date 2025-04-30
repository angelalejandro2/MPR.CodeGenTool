using System.Collections.Generic;
using MPR.CodeGenTool.Domain.Metadata.Common;

namespace MPR.CodeGenTool.Domain.Metadata.Generation
{
    /// <summary>
    /// Represents a controller for an entity
    /// </summary>
    public class ControllerMetadata
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string EntityName { get; set; }
        public string ServiceName { get; set; }
        public string ServiceNamespace { get; set; }
        public string DtoName { get; set; }
        public string CreateDtoName { get; set; }
        public string UpdateDtoName { get; set; }
        public string Route { get; set; }
        public bool HasCompositeKey { get; set; }
        public List<PropertyMetadata> KeyProperties { get; set; } = new();
        public bool RequiresAuthorization { get; set; }
        public Dictionary<string, string> AuthorizationPolicies { get; set; } = new();
    }
}