using System.Collections.Generic;
using MPR.CodeGenTool.Domain.Metadata.Common;

namespace MPR.CodeGenTool.Domain.Metadata.Generation
{
    /// <summary>
    /// Represents a DTO generated from an entity
    /// </summary>
    public class DtoMetadata
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string EntityName { get; set; }
        public string EntityNamespace { get; set; }
        public List<PropertyMetadata> Properties { get; set; } = new();
        public List<AttributeMetadata> Attributes { get; set; } = new();
    }
}