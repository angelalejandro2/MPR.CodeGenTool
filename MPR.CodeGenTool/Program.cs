using MPR.CodeGenTool.Services;

var command = args.Length > 0 ? args[0] : "";

switch (command.ToLower())
{
    case "create":
        var solutionName = args.Length > 1 ? args[1] : "MyApi";
        ProjectScaffolder.CreateSolution(solutionName);
        break;

    case "generate":
        var projectName = args.Length > 1 ? args[1] : "MyApi";
        CodeGenerator.GenerateFromContexts(projectName);
        break;

    default:
        Console.WriteLine("Uso: dotnet run --project MPR.CodeGenTool -- [create|generate] NombreProyecto");
        break;
}