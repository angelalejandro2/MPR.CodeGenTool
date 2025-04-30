using System.Collections.Generic;
using MPR.CodeGenTool.Domain.Metadata.Common;

namespace MPR.CodeGenTool.Domain.Metadata.Generation
{
    /// <summary>
    /// Represents a repository interface for an entity
    /// </summary>
    public class RepositoryInterfaceMetadata
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string EntityName { get; set; }
        public string EntityNamespace { get; set; }
        public bool HasCompositeKey { get; set; }
        public List<PropertyMetadata> KeyProperties { get; set; } = new();
    }

    /// <summary>
    /// Represents a repository implementation for an entity
    /// </summary>
    public class RepositoryImplementationMetadata
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string InterfaceName { get; set; }
        public string InterfaceNamespace { get; set; }
        public string EntityName { get; set; }
        public string EntityNamespace { get; set; }
        public string DbContextName { get; set; }
        public string DbContextNamespace { get; set; }
    }
}