using System.Collections.Generic;
using MPR.CodeGenTool.Domain.Metadata.Common;

namespace MPR.CodeGenTool.Domain.Metadata
{
    /// <summary>
    /// Represents a source code file
    /// </summary>
    public class FileMetadata
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public string Namespace { get; set; }
        public List<TypeMetadata> Types { get; set; } = new();
    }
}