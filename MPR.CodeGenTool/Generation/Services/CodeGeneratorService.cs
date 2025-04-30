// Generation/Services/CodeGeneratorService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MPR.CodeGenTool.Domain.Configuration;
using MPR.CodeGenTool.Domain.Metadata;
using MPR.CodeGenTool.Domain.Metadata.Context;
using MPR.CodeGenTool.Domain.Metadata.Entity;
using MPR.CodeGenTool.Domain.Metadata.Generation;
using MPR.CodeGenTool.Infrastructure.FileSystem;

namespace MPR.CodeGenTool.Generation.Services
{
    public class CodeGeneratorService
    {
        private readonly TemplateManagerService _templateManager;
        private readonly FileService _fileService;
        
        public CodeGeneratorService(TemplateManagerService templateManager, FileService fileService)
        {
            _templateManager = templateManager;
            _fileService = fileService;
        }
        
        public async Task GenerateCodeAsync(SolutionMetadata solution, CodeGenerationConfig config, string outputDirectory)
        {
            // Create output directory structure
            await CreateDirectoryStructureAsync(outputDirectory);
            
            // Extract all entities from the solution
            var entities = GetAllEntities(solution);
            
            // Extract all DbContexts from the solution
            var dbContexts = GetAllDbContexts(solution);
            
            // Generate code for each entity
            foreach (var entity in entities)
            {
                await GenerateEntityCodeAsync(entity, config, outputDirectory);
            }
            
            // Generate unit of work
            await GenerateUnitOfWorkAsync(entities, dbContexts, config, outputDirectory);
            
            // Generate startup registrations
            await GenerateRegistrationsAsync(entities, dbContexts, config, outputDirectory);
        }
        
        private async Task CreateDirectoryStructureAsync(string outputDirectory)
        {
            var directories = new[]
            {
                // Domain layer
                Path.Combine(outputDirectory, "Domain", "Entities"),
                Path.Combine(outputDirectory, "Domain", "Interfaces"),
                Path.Combine(outputDirectory, "Domain", "Interfaces", "Repositories"),
                
                // Application layer
                Path.Combine(outputDirectory, "Application", "DTOs"),
                Path.Combine(outputDirectory, "Application", "Mappings"),
                Path.Combine(outputDirectory, "Application", "Services"),
                Path.Combine(outputDirectory, "Application", "Validators"),
                
                // Infrastructure layer
                Path.Combine(outputDirectory, "Infrastructure", "Context"),
                Path.Combine(outputDirectory, "Infrastructure", "Repositories"),
                
                // API layer
                Path.Combine(outputDirectory, "Api", "Controllers"),
                Path.Combine(outputDirectory, "Api", "Startup")
            };
            
            foreach (var directory in directories)
            {
                await _fileService.CreateDirectoryAsync(directory);
            }
        }
        
        private List<EntityMetadata> GetAllEntities(SolutionMetadata solution)
        {
            var entities = new List<EntityMetadata>();
            
            foreach (var project in solution.Projects)
            {
                foreach (var file in project.Files)
                {
                    foreach (var type in file.Types)
                    {
                        if (type is EntityMetadata entity)
                        {
                            entities.Add(entity);
                        }
                    }
                }
            }
            
            return entities;
        }
        
        private List<DbContextMetadata> GetAllDbContexts(SolutionMetadata solution)
        {
            var dbContexts = new List<DbContextMetadata>();
            
            foreach (var project in solution.Projects)
            {
                foreach (var file in project.Files)
                {
                    foreach (var type in file.Types)
                    {
                        if (type is DbContextMetadata dbContext)
                        {
                            dbContexts.Add(dbContext);
                        }
                    }
                }
            }
            
            return dbContexts;
        }
        
        private async Task GenerateEntityCodeAsync(EntityMetadata entity, CodeGenerationConfig config, string outputDirectory)
        {
            // Check if we should generate code for this entity
            if (config.Entities.TryGetValue(entity.Name, out var entityConfig) && 
                (!entityConfig.GenerateService && !entityConfig.GenerateControllers))
            {
                return;
            }
            
            // Generate DTOs
            await GenerateDtosAsync(entity, config, outputDirectory);
            
            // Generate repository interface
            await GenerateRepositoryInterfaceAsync(entity, config, outputDirectory);
            
            // Generate repository implementation
            await GenerateRepositoryImplementationAsync(entity, config, outputDirectory);
            
            // Generate service
            if (!entityConfig?.GenerateService ?? true)
            {
                await GenerateServiceAsync(entity, config, outputDirectory);
            }
            
            // Generate controller
            if (!entityConfig?.GenerateControllers ?? true)
            {
                await GenerateControllerAsync(entity, config, outputDirectory);
            }
        }
        
        private async Task GenerateDtosAsync(EntityMetadata entity, CodeGenerationConfig config, string outputDirectory)
        {
            // Create DTO metadata
            var dtoName = GetDtoName(entity.Name, config);
            var createDtoName = GetCreateDtoName(entity.Name, config);
            var updateDtoName = GetUpdateDtoName(entity.Name, config);
            
            var dtoMetadata = new DtoMetadata
            {
                Name = dtoName,
                Namespace = $"{config.RootNamespace}.Application.DTOs",
                EntityName = entity.Name,
                EntityNamespace = $"{config.RootNamespace}.Domain.Entities",
                Properties = entity.Properties
            };
            
            var createDtoMetadata = new DtoMetadata
            {
                Name = createDtoName,
                Namespace = $"{config.RootNamespace}.Application.DTOs",
                EntityName = entity.Name,
                EntityNamespace = $"{config.RootNamespace}.Domain.Entities",
                Properties = entity.NonDatabaseGeneratedProperties.ToList()
            };
            
            var updateDtoMetadata = new DtoMetadata
            {
                Name = updateDtoName,
                Namespace = $"{config.RootNamespace}.Application.DTOs",
                EntityName = entity.Name,
                EntityNamespace = $"{config.RootNamespace}.Domain.Entities",
                Properties = entity.UpdateableProperties.ToList()
            };
            
            // Render templates
            var dtoContent = await _templateManager.RenderTemplateAsync("Application/DTOs/Dto.scriban", dtoMetadata);
            var createDtoContent = await _templateManager.RenderTemplateAsync("Application/DTOs/CreateDto.scriban", createDtoMetadata);
            var updateDtoContent = await _templateManager.RenderTemplateAsync("Application/DTOs/UpdateDto.scriban", updateDtoMetadata);
            
            // Write files
            var dtoPath = Path.Combine(outputDirectory, "Application", "DTOs", $"{dtoName}.cs");
            var createDtoPath = Path.Combine(outputDirectory, "Application", "DTOs", $"{createDtoName}.cs");
            var updateDtoPath = Path.Combine(outputDirectory, "Application", "DTOs", $"{updateDtoName}.cs");
            
            await _fileService.WriteAllTextAsync(dtoPath, dtoContent);
            await _fileService.WriteAllTextAsync(createDtoPath, createDtoContent);
            await _fileService.WriteAllTextAsync(updateDtoPath, updateDtoContent);
            
            // Generate mapping profile
            await GenerateMappingProfileAsync(entity, dtoName, createDtoName, updateDtoName, config, outputDirectory);
        }
        
        private async Task GenerateMappingProfileAsync(
            EntityMetadata entity, 
            string dtoName, 
            string createDtoName, 
            string updateDtoName, 
            CodeGenerationConfig config, 
            string outputDirectory)
        {
            var mappingMetadata = new
            {
                EntityName = entity.Name,
                EntityNamespace = $"{config.RootNamespace}.Domain.Entities",
                DtoName = dtoName,
                CreateDtoName = createDtoName,
                UpdateDtoName = updateDtoName,
                Namespace = $"{config.RootNamespace}.Application.Mappings",
                Properties = entity.Properties,
                MappingName = $"{entity.Name}MappingProfile"
            };
            
            var mappingContent = await _templateManager.RenderTemplateAsync("Application/Mappings/MappingProfile.scriban", mappingMetadata);
            var mappingPath = Path.Combine(outputDirectory, "Application", "Mappings", $"{entity.Name}MappingProfile.cs");
            
            await _fileService.WriteAllTextAsync(mappingPath, mappingContent);
        }
        
        private async Task GenerateRepositoryInterfaceAsync(EntityMetadata entity, CodeGenerationConfig config, string outputDirectory)
        {
            var repositoryInterfaceMetadata = new RepositoryInterfaceMetadata
            {
                Name = $"I{entity.Name}Repository",
                Namespace = $"{config.RootNamespace}.Domain.Interfaces.Repositories",
                EntityName = entity.Name,
                EntityNamespace = $"{config.RootNamespace}.Domain.Entities",
                HasCompositeKey = entity.HasCompositeKey,
                KeyProperties = entity.KeyProperties
            };
            
            var content = await _templateManager.RenderTemplateAsync("Domain/Interfaces/Repositories/IRepository.scriban", repositoryInterfaceMetadata);
            var path = Path.Combine(outputDirectory, "Domain", "Interfaces", "Repositories", $"{repositoryInterfaceMetadata.Name}.cs");
            
            await _fileService.WriteAllTextAsync(path, content);
        }
        
        private async Task GenerateRepositoryImplementationAsync(EntityMetadata entity, CodeGenerationConfig config, string outputDirectory)
        {
            // Find the DbContext that contains this entity
            var dbContextName = "AppDbContext"; // Default if not found
            
            var repositoryMetadata = new RepositoryImplementationMetadata
            {
                Name = $"{entity.Name}Repository",
                Namespace = $"{config.RootNamespace}.Infrastructure.Repositories",
                InterfaceName = $"I{entity.Name}Repository",
                InterfaceNamespace = $"{config.RootNamespace}.Domain.Interfaces.Repositories",
                EntityName = entity.Name,
                EntityNamespace = $"{config.RootNamespace}.Domain.Entities",
                DbContextName = dbContextName,
                DbContextNamespace = $"{config.RootNamespace}.Infrastructure.Context"
            };
            
            var content = await _templateManager.RenderTemplateAsync("Infrastructure/Repositories/Repository.scriban", repositoryMetadata);
            var path = Path.Combine(outputDirectory, "Infrastructure", "Repositories", $"{repositoryMetadata.Name}.cs");
            
            await _fileService.WriteAllTextAsync(path, content);
        }
        
        private async Task GenerateServiceAsync(EntityMetadata entity, CodeGenerationConfig config, string outputDirectory)
        {
            var dtoName = GetDtoName(entity.Name, config);
            var createDtoName = GetCreateDtoName(entity.Name, config);
            var updateDtoName = GetUpdateDtoName(entity.Name, config);
            
            var serviceMetadata = new ServiceMetadata
            {
                Name = $"{entity.Name}Service",
                Namespace = $"{config.RootNamespace}.Application.Services",
                EntityName = entity.Name,
                EntityNamespace = $"{config.RootNamespace}.Domain.Entities",
                DtoName = dtoName,
                CreateDtoName = createDtoName,
                UpdateDtoName = updateDtoName,
                RepositoryInterfaceName = $"I{entity.Name}Repository",
                HasCompositeKey = entity.HasCompositeKey,
                KeyProperties = entity.KeyProperties
            };
            
            var content = await _templateManager.RenderTemplateAsync("Application/Services/Service.scriban", serviceMetadata);
            var path = Path.Combine(outputDirectory, "Application", "Services", $"{serviceMetadata.Name}.cs");
            
            await _fileService.WriteAllTextAsync(path, content);
        }
        
        private async Task GenerateControllerAsync(EntityMetadata entity, CodeGenerationConfig config, string outputDirectory)
        {
            var dtoName = GetDtoName(entity.Name, config);
            var createDtoName = GetCreateDtoName(entity.Name, config);
            var updateDtoName = GetUpdateDtoName(entity.Name, config);
            
            var controllerMetadata = new ControllerMetadata
            {
                Name = $"{entity.Name}Controller",
                Namespace = $"{config.RootNamespace}.Api.Controllers",
                EntityName = entity.Name,
                ServiceName = $"{entity.Name}Service",
                ServiceNamespace = $"{config.RootNamespace}.Application.Services",
                DtoName = dtoName,
                CreateDtoName = createDtoName,
                UpdateDtoName = updateDtoName,
                Route = entity.Name.ToLowerInvariant(),
                HasCompositeKey = entity.HasCompositeKey,
                KeyProperties = entity.KeyProperties,
                RequiresAuthorization = config.Defaults.DefaultPolicies.Count > 0
            };
            
            // Add authorization policies
            if (config.Entities.TryGetValue(entity.Name, out var entityConfig) && entityConfig.Policies.Count > 0)
            {
                controllerMetadata.AuthorizationPolicies = entityConfig.Policies;
            }
            else if (config.Defaults.DefaultPolicies.Count > 0)
            {
                controllerMetadata.AuthorizationPolicies = config.Defaults.DefaultPolicies;
            }
            
            var content = await _templateManager.RenderTemplateAsync("Api/Controllers/Controller.scriban", controllerMetadata);
            var path = Path.Combine(outputDirectory, "Api", "Controllers", $"{controllerMetadata.Name}.cs");
            
            await _fileService.WriteAllTextAsync(path, content);
        }
        
        private async Task GenerateUnitOfWorkAsync(
            List<EntityMetadata> entities, 
            List<DbContextMetadata> dbContexts, 
            CodeGenerationConfig config, 
            string outputDirectory)
        {
            // Generate IUnitOfWork interface
            var repositoryInterfaces = entities.Select(e => new RepositoryInterfaceMetadata
            {
                Name = $"I{e.Name}Repository",
                Namespace = $"{config.RootNamespace}.Domain.Interfaces.Repositories",
                EntityName = e.Name,
                EntityNamespace = $"{config.RootNamespace}.Domain.Entities",
                HasCompositeKey = e.HasCompositeKey,
                KeyProperties = e.KeyProperties
            }).ToList();
            
            var unitOfWorkInterfaceMetadata = new UnitOfWorkInterfaceMetadata
            {
                Name = "IUnitOfWork",
                Namespace = $"{config.RootNamespace}.Domain.Interfaces",
                Repositories = repositoryInterfaces
            };
            
            var interfaceContent = await _templateManager.RenderTemplateAsync("Domain/Interfaces/IUnitOfWork.scriban", unitOfWorkInterfaceMetadata);
            var interfacePath = Path.Combine(outputDirectory, "Domain", "Interfaces", "IUnitOfWork.cs");
            
            await _fileService.WriteAllTextAsync(interfacePath, interfaceContent);
            
            // Generate UnitOfWork implementation
            var repositoryImplementations = entities.Select(e => new RepositoryImplementationMetadata
            {
                Name = $"{e.Name}Repository",
                Namespace = $"{config.RootNamespace}.Infrastructure.Repositories",
                InterfaceName = $"I{e.Name}Repository",
                InterfaceNamespace = $"{config.RootNamespace}.Domain.Interfaces.Repositories",
                EntityName = e.Name,
                EntityNamespace = $"{config.RootNamespace}.Domain.Entities",
                DbContextName = "AppDbContext", // Default
                DbContextNamespace = $"{config.RootNamespace}.Infrastructure.Context"
            }).ToList();
            
            var unitOfWorkImplementationMetadata = new UnitOfWorkImplementationMetadata
            {
                Name = "UnitOfWork",
                Namespace = $"{config.RootNamespace}.Infrastructure",
                InterfaceName = "IUnitOfWork",
                InterfaceNamespace = $"{config.RootNamespace}.Domain.Interfaces",
                DbContexts = dbContexts,
                Repositories = repositoryImplementations
            };
            
            var implementationContent = await _templateManager.RenderTemplateAsync("Infrastructure/UnitOfWork.scriban", unitOfWorkImplementationMetadata);
            var implementationPath = Path.Combine(outputDirectory, "Infrastructure", "UnitOfWork.cs");
            
            await _fileService.WriteAllTextAsync(implementationPath, implementationContent);
        }
        
        private async Task GenerateRegistrationsAsync(
            List<EntityMetadata> entities, 
            List<DbContextMetadata> dbContexts, 
            CodeGenerationConfig config, 
            string outputDirectory)
        {
            // Generate ApplicationServiceRegistration
            var serviceRegistrationMetadata = new
            {
                Namespace = $"{config.RootNamespace}.Api.Startup",
                Services = entities.Select(e => $"{e.Name}Service").ToList(),
                RootNamespace = config.RootNamespace
            };
            
            var serviceRegistrationContent = await _templateManager.RenderTemplateAsync("Api/Startup/ApplicationServiceRegistration.scriban", serviceRegistrationMetadata);
            var serviceRegistrationPath = Path.Combine(outputDirectory, "Api", "Startup", "ApplicationServiceRegistration.cs");
            
            await _fileService.WriteAllTextAsync(serviceRegistrationPath, serviceRegistrationContent);
            
            // Generate DbContextRegistration
            var dbContextRegistrationMetadata = new
            {
                Namespace = $"{config.RootNamespace}.Api.Startup",
                DbContexts = dbContexts,
                RootNamespace = config.RootNamespace
            };
            
            var dbContextRegistrationContent = await _templateManager.RenderTemplateAsync("Api/Startup/DbContextRegistration.scriban", dbContextRegistrationMetadata);
            var dbContextRegistrationPath = Path.Combine(outputDirectory, "Api", "Startup", "DbContextRegistration.cs");
            
            await _fileService.WriteAllTextAsync(dbContextRegistrationPath, dbContextRegistrationContent);
        }
        
        private string GetDtoName(string entityName, CodeGenerationConfig config)
        {
            if (config.Entities.TryGetValue(entityName, out var entityConfig) && !string.IsNullOrEmpty(entityConfig.Name))
            {
                return $"{entityConfig.Name}Dto";
            }
            
            return $"{entityName}Dto";
        }
        
        private string GetCreateDtoName(string entityName, CodeGenerationConfig config)
        {
            if (config.Entities.TryGetValue(entityName, out var entityConfig) && !string.IsNullOrEmpty(entityConfig.Name))
            {
                return $"Create{entityConfig.Name}Dto";
            }
            
            return $"{entityName}CreateDto";
        }
        
        private string GetUpdateDtoName(string entityName, CodeGenerationConfig config)
        {
            if (config.Entities.TryGetValue(entityName, out var entityConfig) && !string.IsNullOrEmpty(entityConfig.Name))
            {
                return $"Update{entityConfig.Name}Dto";
            }
            
            return $"{entityName}UpdateDto";
        }
    }
}