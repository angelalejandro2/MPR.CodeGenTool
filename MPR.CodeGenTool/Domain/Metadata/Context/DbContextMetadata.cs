using System.Collections.Generic;
using MPR.CodeGenTool.Domain.Metadata.Common;

namespace MPR.CodeGenTool.Domain.Metadata.Context
{
    /// <summary>
    /// Represents a DbContext class in the infrastructure layer
    /// </summary>
    public class DbContextMetadata : TypeMetadata
    {
        public string ConnectionStringName { get; set; }
        public DatabaseProvider Provider { get; set; }
        public List<DbSetMetadata> DbSets { get; set; } = new();
    }
}