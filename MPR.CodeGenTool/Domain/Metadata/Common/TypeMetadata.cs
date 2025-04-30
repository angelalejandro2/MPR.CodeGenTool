using System.Collections.Generic;

namespace MPR.CodeGenTool.Domain.Metadata.Common
{
    /// <summary>
    /// Base class for all type definitions (classes, interfaces, enums)
    /// </summary>
    public abstract class TypeMetadata
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public bool IsPartial { get; set; }
        public bool IsPublic { get; set; }
        public List<AttributeMetadata> Attributes { get; set; } = new();
    }
}