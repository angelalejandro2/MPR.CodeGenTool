using System.Collections.Generic;
using MPR.CodeGenTool.Domain.Metadata.Common;

namespace MPR.CodeGenTool.Domain.Metadata.Generation
{
    /// <summary>
    /// Represents a service for an entity
    /// </summary>
    public class ServiceMetadata
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string EntityName { get; set; }
        public string EntityNamespace { get; set; }
        public string DtoName { get; set; }
        public string CreateDtoName { get; set; }
        public string UpdateDtoName { get; set; }
        public string RepositoryInterfaceName { get; set; }
        public bool HasCompositeKey { get; set; }
        public List<PropertyMetadata> KeyProperties { get; set; } = new();
    }
}