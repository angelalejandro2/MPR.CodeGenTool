// Cli/Commands/FullCommand.cs
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using MPR.CodeGenTool.Analysis.Services;
using MPR.CodeGenTool.Generation.Services;
using MPR.CodeGenTool.Infrastructure.Configuration;
using MPR.CodeGenTool.Infrastructure.FileSystem;

namespace MPR.CodeGenTool.Cli.Commands
{
    public class FullCommand : Command
    {
        private readonly Option<FileInfo> _solutionOption;
        private readonly Option<DirectoryInfo> _outputOption;
        private readonly Option<FileInfo> _configOption;
        private readonly Option<DirectoryInfo> _templatePathOption;
        
        public FullCommand() : base("full", "Analyze and generate code in one step")
        {
            _solutionOption = new Option<FileInfo>(
                new[] { "--solution", "-s" },
                "Path to the solution file");
            _solutionOption.IsRequired = true;
            
            _outputOption = new Option<DirectoryInfo>(
                new[] { "--output", "-o" },
                "Output directory for generated code");
            _outputOption.IsRequired = true;
            
            _configOption = new Option<FileInfo>(
                new[] { "--config", "-c" },
                "Path to configuration file");
            _configOption.SetDefaultValue(new FileInfo("mpr.codegen.json"));
            
            _templatePathOption = new Option<DirectoryInfo>(
                new[] { "--templates", "-t" },
                "Path to template directory");
            
            AddOption(_solutionOption);
            AddOption(_outputOption);
            AddOption(_configOption);
            AddOption(_templatePathOption);
            
            this.SetHandler(ExecuteAsync);
        }
        
        private async Task ExecuteAsync(InvocationContext context)
        {
            var solution = context.ParseResult.GetValueForOption(_solutionOption);
            var output = context.ParseResult.GetValueForOption(_outputOption);
            var config = context.ParseResult.GetValueForOption(_configOption);
            var templatePath = context.ParseResult.GetValueForOption(_templatePathOption);
            
            // Rest of your method...
        }
    }
}