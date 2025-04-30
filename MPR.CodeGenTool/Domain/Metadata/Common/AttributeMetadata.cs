using System.Collections.Generic;

namespace MPR.CodeGenTool.Domain.Metadata.Common
{
    /// <summary>
    /// Represents an attribute applied to a type or property
    /// </summary>
    public class AttributeMetadata
    {
        public string Name { get; set; }
        public List<AttributeArgumentMetadata> Arguments { get; set; } = new();
    }

    /// <summary>
    /// Represents an argument for an attribute
    /// </summary>
    public class AttributeArgumentMetadata
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsNamedArgument { get; set; }
    }
}