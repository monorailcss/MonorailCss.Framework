# MonorailCSS

MonorailCss is a JIT CSS compiler that aims to be a Tailwind CSS 4.1 compatible CSS engine written in .NET. It processes utility class names and generates optimized CSS output.

## Architecture Overview

### Core Components

1. **CssFramework** (`src/MonorailCss/CssFramework.cs`): Main entry point that orchestrates the entire processing pipeline. It handles parsing, compilation, post-processing, and CSS generation.

2. **Utilities System** (`src/MonorailCss/Utilities/`): Self-contained components that compile CSS class candidates into AST nodes. Each utility handles specific CSS properties:
   - Static utilities extend `BaseStaticUtility` for fixed property/value mappings
   - Functional utilities extend `BaseFunctionalUtility` for theme-aware dynamic values
   - Specialized base classes exist for common patterns (colors, spacing, filters)

3. **Candidate System** (`src/MonorailCss/Candidates/`): Represents parsed utility classes with variants and modifiers. The `CandidateParser` tokenizes and structures raw class names.

4. **AST (Abstract Syntax Tree)** (`src/MonorailCss/Ast/`): Intermediate representation of CSS rules before final generation. Includes `Declaration`, `Rule`, `ComponentRule` nodes.

5. **Processing Pipeline** (`src/MonorailCss/Pipeline/`): Multi-stage transformation pipeline that processes AST nodes through:
   - Theme variable tracking
   - Arbitrary value validation
   - Negative value normalization
   - Color modifier processing
   - Important flag handling
   - Variable fallback resolution
   - Declaration merging
   - Media query consolidation
   - Layer assignment

6. **Theme System** (`src/MonorailCss/Theme/`): Manages design tokens and CSS custom properties. Supports theme customization and usage tracking for optimization.

7. **Variant System** (`src/MonorailCss/Variants/`): Handles pseudo-classes, media queries, and other CSS modifiers. Built-in variants are automatically registered.

### Key Design Patterns

- **Auto-Discovery**: Utilities are automatically discovered via reflection at startup
- **Priority System**: Utilities have priorities (0-1000) determining evaluation order
- **Immutable Data**: Extensive use of immutable collections
- **Pipeline Architecture**: Modular stages for processing transformations
- **Theme Resolution**: Values resolved through namespace chains with fallback support
- **Arbitrary Values**: Support for user-defined values in square brackets `[value]`

### Project Structure

```
MonorailCss.Framework/
├── src/
│   └── MonorailCss/          # Core library
│       ├── Ast/              # AST node types
│       ├── Candidates/       # Candidate parsing
│       ├── Css/              # CSS generation
│       ├── Parser/           # Class name parsing
│       ├── Pipeline/         # Processing pipeline
│       ├── Processing/       # Post-processing
│       ├── Sorting/          # CSS output sorting
│       ├── Theme/            # Theme management
│       ├── Utilities/        # Utility implementations
│       └── Variants/         # Variant handling
└── tests/
    ├── MonorailCss.Tests/    # Integration tests
    ├── MonorailCss.Interactive/  # Interactive testing
    ├── MonorailCss.Benchmarks/   # Performance benchmarks
    └── TryMonorail/          # Blazor playground app
```

## Commands

### Build and Development
```bash
# Build the solution
dotnet build
```

### Testing
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests matching a filter
dotnet test --filter "FullyQualifiedName~OpacityUtility"
```

### Interactive Development

Use the Interactive project to verify output. Pass the utilities to test in on the command line

```bash
# Run the interactive test project
dotnet run --project tests/MonorailCss.Interactive/MonorailCss.Interactive.csproj -- bg-red-500
```

## Testing Strategy

The project uses integration testing rather than unit testing individual utilities. The main test file `tests/MonorailCss.Tests/Interactive/AllUtilitiesIntegrationTest.cs` verifies that utilities generate expected CSS properties.

When adding new utilities:
1. Create the utility class in appropriate category folder
2. Add test cases to `AllUtilitiesIntegrationTest.cs`
3. Verify all tests pass before committing