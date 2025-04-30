using MPR.CodeGenTool.Domain.Metadata.Entity;

namespace MPR.CodeGenTool.Domain.Metadata.Context
{
    /// <summary>
    /// Represents a DbSet property in a DbContext
    /// </summary>
    public class DbSetMetadata
    {
        public string Name { get; set; }
        public string EntityTypeName { get; set; }
        public EntityMetadata EntityType { get; set; }
    }
}