// Cli/Commands/AnalyzeCommand.cs
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using MPR.CodeGenTool.Analysis.Services;
using MPR.CodeGenTool.Infrastructure.Configuration;

namespace MPR.CodeGenTool.Cli.Commands
{
    public class AnalyzeCommand : Command
    {
        private readonly Option<FileInfo> _solutionOption;
        private readonly Option<DirectoryInfo> _outputOption;
        private readonly Option<FileInfo> _configOption;
        
        public AnalyzeCommand() : base("analyze", "Analyze source code and generate metadata")
        {
            _solutionOption = new Option<FileInfo>(
                new[] { "--solution", "-s" },
                "Path to the solution file");
            _solutionOption.IsRequired = true;
            
            _outputOption = new Option<DirectoryInfo>(
                new[] { "--output", "-o" },
                "Output directory for metadata");
            _outputOption.IsRequired = true;
            
            _configOption = new Option<FileInfo>(
                new[] { "--config", "-c" },
                "Path to configuration file");
            _configOption.SetDefaultValue(new FileInfo("mpr.codegen.json"));
            
            AddOption(_solutionOption);
            AddOption(_outputOption);
            AddOption(_configOption);
            
            this.SetHandler(ExecuteAsync);
        }
        
        private async Task ExecuteAsync(InvocationContext context)
        {
            var solution = context.ParseResult.GetValueForOption(_solutionOption);
            var output = context.ParseResult.GetValueForOption(_outputOption);
            var config = context.ParseResult.GetValueForOption(_configOption);
            
            // Rest of your method...
            // Create services
            var analysisService = new SolutionAnalysisService();
            var configService = new ConfigurationService();
            
            // Load configuration
            var configData = await configService.LoadConfigAsync(config.FullName);
            
            // Analyze solution
            Console.WriteLine($"Analyzing solution: {solution.FullName}");
            var solutionMetadata = await analysisService.AnalyzeSolutionAsync(solution.FullName);
            
            // Save metadata
            await configService.SaveMetadataAsync(solutionMetadata, Path.Combine(output.FullName, "metadata.json"));
            
            Console.WriteLine($"Analysis completed. Metadata saved to {Path.Combine(output.FullName, "metadata.json")}");
        }
    }
}