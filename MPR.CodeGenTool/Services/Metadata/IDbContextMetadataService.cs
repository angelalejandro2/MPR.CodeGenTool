using MPR.CodeGenTool.Models;

namespace MPR.CodeGenTool.Services.Metadata;

public interface IDbContextMetadataService
{
    List<EntityMetadata> LoadMetadataFromAssembly(string assemblyPath);
}