// Analysis/Roslyn/IRoslynAnalyzer.cs
namespace MPR.CodeGenTool.Analysis.Roslyn
{
    public interface IRoslynAnalyzer<T>
    {
        T Analyze(Microsoft.CodeAnalysis.SyntaxNode node);
        bool CanAnalyze(Microsoft.CodeAnalysis.SyntaxNode node);
    }
}