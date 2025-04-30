// Analysis/Services/SolutionAnalysisService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using MPR.CodeGenTool.Analysis.Roslyn;
using MPR.CodeGenTool.Analysis.Roslyn.Analyzers;
using MPR.CodeGenTool.Domain.Metadata;
using MPR.CodeGenTool.Domain.Metadata.Context;
using MPR.CodeGenTool.Domain.Metadata.Entity;

namespace MPR.CodeGenTool.Analysis.Services
{
    public class SolutionAnalysisService
    {
        private readonly EntityAnalyzer _entityAnalyzer;
        private readonly DbContextAnalyzer _dbContextAnalyzer;
        
        public SolutionAnalysisService()
        {
            // Register MSBuild instance
            if (!MSBuildLocator.IsRegistered)
                MSBuildLocator.RegisterDefaults();
            
            _entityAnalyzer = new EntityAnalyzer();
            _dbContextAnalyzer = new DbContextAnalyzer();
        }
        
        public async Task<SolutionMetadata> AnalyzeSolutionAsync(string solutionPath)
        {
            if (!File.Exists(solutionPath))
                throw new FileNotFoundException($"Solution file not found: {solutionPath}");
            
            using var workspace = MSBuildWorkspace.Create();
            
            // Subscribe to workspace failure events
            workspace.WorkspaceFailed += (s, e) => 
                Console.WriteLine($"Workspace error: {e.Diagnostic.Message}");
            
            Console.WriteLine($"Loading solution: {solutionPath}");
            var solution = await workspace.OpenSolutionAsync(solutionPath);
            
            var solutionName = Path.GetFileNameWithoutExtension(solutionPath);
            var rootNamespace = solutionName;
            
            var solutionMetadata = new SolutionMetadata
            {
                Name = solutionName,
                RootNamespace = rootNamespace,
                Projects = new List<ProjectMetadata>()
            };
            
            foreach (var project in solution.Projects)
            {
                var projectMetadata = await AnalyzeProjectAsync(project);
                solutionMetadata.Projects.Add(projectMetadata);
            }
            
            // Link DbSets to EntityMetadata
            LinkDbSetsToEntities(solutionMetadata);
            
            return solutionMetadata;
        }
        
        private async Task<ProjectMetadata> AnalyzeProjectAsync(Project project)
        {
            var projectType = DetermineProjectType(project.Name);
            
            var projectMetadata = new ProjectMetadata
            {
                Name = project.Name,
                RelativePath = project.FilePath,
                Type = projectType,
                Files = new List<FileMetadata>()
            };
            
            // Compile the project to get semantic model
            var compilation = await project.GetCompilationAsync();
            if (compilation == null)
                return projectMetadata;
            
            foreach (var document in project.Documents)
            {
                if (Path.GetExtension(document.FilePath) != ".cs")
                    continue;
                
                var syntaxTree = await document.GetSyntaxTreeAsync();
                if (syntaxTree == null)
                    continue;
                
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var fileMetadata = AnalyzeFile(document, syntaxTree, semanticModel);
                
                if (fileMetadata.Types.Any())
                {
                    projectMetadata.Files.Add(fileMetadata);
                }
            }
            
            return projectMetadata;
        }
        
        private FileMetadata AnalyzeFile(Document document, SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var root = syntaxTree.GetRoot();
            var fileName = Path.GetFileName(document.FilePath);
            
            var fileMetadata = new FileMetadata
            {
                Name = fileName,
                RelativePath = document.FilePath,
                Namespace = ExtractNamespace(root),
                Types = new List<TypeMetadata>()
            };
            
            // Find all class declarations in the file
            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            
            foreach (var classDeclaration in classDeclarations)
            {
                if (_entityAnalyzer.CanAnalyze(classDeclaration))
                {
                    var entityMetadata = _entityAnalyzer.Analyze(classDeclaration);
                    fileMetadata.Types.Add(entityMetadata);
                }
                else if (_dbContextAnalyzer.CanAnalyze(classDeclaration))
                {
                    var dbContextMetadata = _dbContextAnalyzer.Analyze(classDeclaration);
                    fileMetadata.Types.Add(dbContextMetadata);
                }
            }
            
            return fileMetadata;
        }
        
        private string ExtractNamespace(SyntaxNode root)
        {
            // Find namespace declaration
            var namespaceDeclaration = root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();
            
            if (namespaceDeclaration != null)
            {
                return namespaceDeclaration.Name.ToString();
            }
            
            // Check for file-scoped namespace (C# 10+)
            var fileScopedNamespace = root.DescendantNodes()
                .OfType<FileScopedNamespaceDeclarationSyntax>()
                .FirstOrDefault();
            
            if (fileScopedNamespace != null)
            {
                return fileScopedNamespace.Name.ToString();
            }
            
            return string.Empty;
        }
        
        private ProjectType DetermineProjectType(string projectName)
        {
            if (projectName.EndsWith(".Api"))
                return ProjectType.Api;
            if (projectName.EndsWith(".Application"))
                return ProjectType.Application;
            if (projectName.EndsWith(".Domain"))
                return ProjectType.Domain;
            if (projectName.EndsWith(".Infrastructure"))
                return ProjectType.Infrastructure;
            if (projectName.EndsWith(".Tests") || projectName.EndsWith(".Test"))
                return ProjectType.Tests;
            
            // Default to Domain if we can't determine
            return ProjectType.Domain;
        }
        
        private void LinkDbSetsToEntities(SolutionMetadata solution)
        {
            var entities = solution.Projects
                .SelectMany(p => p.Files)
                .SelectMany(f => f.Types)
                .OfType<EntityMetadata>()
                .ToDictionary(e => e.Name);
            
            var dbContexts = solution.Projects
                .SelectMany(p => p.Files)
                .SelectMany(f => f.Types)
                .OfType<DbContextMetadata>();
            
            foreach (var dbContext in dbContexts)
            {
                foreach (var dbSet in dbContext.DbSets)
                {
                    if (entities.TryGetValue(dbSet.EntityTypeName, out var entity))
                    {
                        dbSet.EntityType = entity;
                    }
                }
            }
        }
    }
}