using System.Collections.Generic;

namespace MPR.CodeGenTool.Domain.Metadata
{
    /// <summary>
    /// Represents a .NET project within the solution
    /// </summary>
    public class ProjectMetadata
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public ProjectType Type { get; set; }
        public List<FileMetadata> Files { get; set; } = new();
    }
}