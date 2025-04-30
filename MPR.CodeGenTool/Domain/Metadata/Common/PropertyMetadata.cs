using System.Collections.Generic;

namespace MPR.CodeGenTool.Domain.Metadata.Common
{
    /// <summary>
    /// Represents a property in a class or interface
    /// </summary>
    public class PropertyMetadata
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsNullable { get; set; }
        public bool IsKey { get; set; }
        public bool IsDatabaseGenerated { get; set; }
        public DatabaseGeneratedOption? DatabaseGeneratedOption { get; set; }
        public bool IsRequired { get; set; }
        public bool IsVirtual { get; set; }
        public List<AttributeMetadata> Attributes { get; set; } = new();
        
        // Utility properties for templates
        public string TypeForDto => IsNullable ? Type : $"{Type}?";
        public string CamelCaseName => char.ToLowerInvariant(Name[0]) + Name.Substring(1);
    }

    /// <summary>
    /// Database generated options for properties
    /// </summary>
    public enum DatabaseGeneratedOption
    {
        None,
        Identity,
        Computed
    }
}