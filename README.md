# MPR.CodeGenTool

# Folder Structure

```
MPR.CodeGenTool/
├── Domain/                           # Core domain models
│   ├── Metadata/                     # Code metadata models
│   │   ├── Common/                   # Shared metadata types
│   │   ├── Entity/                   # Entity-specific metadata
│   │   ├── Context/                  # DbContext metadata
│   │   └── Generation/               # Generation-specific metadata
│   ├── Configuration/                # Configuration models
│   └── Extensions/                   # Domain model extensions
│
├── Analysis/                         # Code analysis functionality
│   ├── Roslyn/                       # Roslyn-based code analysis
│   │   ├── Analyzers/                # Specific analyzers (entities, contexts)
│   │   └── Extensions/               # Extensions for Roslyn
│   ├── Services/                     # Analysis orchestration services
│   └── Mappers/                      # Map syntax to domain models
│
├── Generation/                       # Code generation functionality
│   ├── Templates/                    # Scriban templates
│   │   ├── Domain/                   # Domain layer templates
│   │   ├── Application/              # Application layer templates
│   │   ├── Infrastructure/           # Infrastructure layer templates
│   │   └── Api/                      # API layer templates
│   ├── Services/                     # Generation services
│   ├── Pipeline/                     # Generation pipeline
│   └── Resolvers/                    # Template resolvers
│
├── Infrastructure/                   # External concerns
│   ├── FileSystem/                   # File system operations
│   ├── Configuration/                # Configuration loading/saving
│   └── Logging/                      # Logging functionality
│
├── Cli/                              # Command-line interface
│   ├── Commands/                     # CLI commands
│   ├── Options/                      # Command options
│   └── Handlers/                     # Command handlers
│
├── Resources/                        # Static resources
│   ├── Templates/                    # Embedded templates
│   └── Defaults/                     # Default configurations
│
└── Common/                           # Shared utilities
    ├── Extensions/                   # Extension methods
    ├── Helpers/                      # Helper classes
    └── Constants/                    # Constant values
```

## Key Folder Explanations
### Domain
Contains all the core domain models that represent the metadata we extract from source code and use for generation. These are pure POCO classes with no dependencies on external frameworks.
### Analysis
Contains everything related to analyzing source code using Roslyn. The analyzers extract information from C# code and map it to our domain models.
### Generation
Contains templates and services for generating code based on the analyzed metadata. Scriban templates are organized by layer in the clean architecture.
### Infrastructure
Handles external concerns like file I/O, logging, and configuration management. This isolates these dependencies from the core business logic.
### CLI
Contains all the command-line interface components, including command definitions, option parsing, and handlers that orchestrate the analysis and generation.
### Resources
Contains static resources used by the application, such as embedded templates and default configurations.
### Common
Contains shared utilities, extensions, and constants used throughout the application.