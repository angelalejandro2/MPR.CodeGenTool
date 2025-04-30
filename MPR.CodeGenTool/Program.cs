// Program.cs
using System;
using System.CommandLine;
using System.Threading.Tasks;
using MPR.CodeGenTool.Cli.Commands;

namespace MPR.CodeGenTool
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("MPR.CodeGenTool - Code generator for Clean Architecture solutions");
            
            rootCommand.AddCommand(new AnalyzeCommand());
            rootCommand.AddCommand(new GenerateCommand());
            rootCommand.AddCommand(new FullCommand());
            
            return await rootCommand.InvokeAsync(args);
        }
    }
}