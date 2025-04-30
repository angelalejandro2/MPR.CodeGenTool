using System.Collections.Generic;
using MPR.CodeGenTool.Domain.Metadata.Context;

namespace MPR.CodeGenTool.Domain.Metadata.Generation
{
    /// <summary>
    /// Represents the unit of work interface
    /// </summary>
    public class UnitOfWorkInterfaceMetadata
    {
        public string Name { get; set; } = "IUnitOfWork";
        public string Namespace { get; set; }
        public List<RepositoryInterfaceMetadata> Repositories { get; set; } = new();
    }

    /// <summary>
    /// Represents the unit of work implementation
    /// </summary>
    public class UnitOfWorkImplementationMetadata
    {
        public string Name { get; set; } = "UnitOfWork";
        public string Namespace { get; set; }
        public string InterfaceName { get; set; } = "IUnitOfWork";
        public string InterfaceNamespace { get; set; }
        public List<DbContextMetadata> DbContexts { get; set; } = new();
        public List<RepositoryImplementationMetadata> Repositories { get; set; } = new();
    }
}