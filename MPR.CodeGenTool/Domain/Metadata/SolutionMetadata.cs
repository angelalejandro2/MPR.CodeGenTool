using System.Collections.Generic;

namespace MPR.CodeGenTool.Domain.Metadata
{
    /// <summary>
    /// Represents a solution that contains multiple projects
    /// </summary>
    public class SolutionMetadata
    {
        public string Name { get; set; }
        public string RootNamespace { get; set; }
        public List<ProjectMetadata> Projects { get; set; } = new();
        public CodeGenerationOptions Options { get; set; } = new();
    }

    /// <summary>
    /// Configuration options for code generation
    /// </summary>
    public class CodeGenerationOptions
    {
        public bool GenerateDtos { get; set; } = true;
        public bool GenerateRepositories { get; set; } = true;
        public bool GenerateServices { get; set; } = true;
        public bool GenerateControllers { get; set; } = true;
        public bool GenerateSwagger { get; set; } = true;
        public bool UseAuthentication { get; set; } = false;
        public bool UseAuthorization { get; set; } = false;
        public bool GenerateTests { get; set; } = true;
        public bool UseFluentValidation { get; set; } = true;
        public bool UseSerilog { get; set; } = true;
    }
}