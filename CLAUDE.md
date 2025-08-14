# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MonorailCSS is a utility-first CSS library inspired by Tailwind CSS. It's a Just-In-Time (JIT) CSS compiler written in C# that generates CSS from a design system and a list of CSS class names.

## Development Commands

### Build
```bash
dotnet build                    # Build entire solution
dotnet build -c Release        # Build in release mode
```

### Test
```bash
dotnet test                     # Run all tests
dotnet test --logger console   # Run tests with console output
```

## Architecture Overview

### Core Components

1. **CssFramework** (`src/MonorailCss/CssFramework.cs`)
   - Main entry point for processing CSS classes
   - Orchestrates the entire compilation pipeline
   - Handles settings, plugin management, and variant processing

2. **DesignSystem** (`src/MonorailCss/DesignSystem.cs`)
   - Configuration container for colors, typography, spacing, etc.
   - Immutable record with hierarchical color definitions
   - Default design system provided with Tailwind-like defaults

3. **Plugin System** (`src/MonorailCss/Plugins/`)
   - Modular architecture where each CSS feature is a plugin
   - Plugins implement `IUtilityPlugin` interface
   - Auto-discovery via reflection from assembly
   - Supports both namespace-based and arbitrary property plugins

4. **Parser System** (`src/MonorailCss/Parser/`)
   - Parses CSS class names into structured syntax objects
   - Handles variants, modifiers, and arbitrary values
   - Supports arbitrary properties like `[color:red]`

5. **Variant System** (`src/MonorailCss/Variants/`)
   - Handles responsive, pseudo-class, and other CSS variants
   - Media queries, hover states, focus states, etc.
   - Extensible through `IVariant` interface

### Plugin Architecture

Plugins are the core extensibility mechanism:

- **Base Plugins**: `BaseUtilityPlugin`, `BaseUtilityNamespacePlugin`
- **Namespace Plugins**: Handle specific CSS property namespaces (e.g., `margin`, `padding`)
- **Color Plugins**: Extend `BaseColorNamespacePlugin` for color-aware utilities
- **Lookup Plugins**: Use dictionaries for simple value mappings
- **Arbitrary Property Plugin**: Handles `[property:value]` syntax

### Key Patterns

1. **Immutable Design**: Heavy use of `ImmutableDictionary` and immutable records
2. **Dependency Injection**: Plugins receive `DesignSystem` and settings via constructor
3. **Caching**: Results cached at multiple levels for performance
4. **Extensibility**: Everything is designed to be extended or overridden

## Project Structure

```
src/
├── MonorailCss/              # Main library
│   ├── Css/                  # CSS object model
│   ├── Framework/            # Core processing logic
│   ├── Parser/               # Class name parsing
│   ├── Plugins/              # All utility plugins
│   │   ├── Backgrounds/      # Background-related utilities
│   │   ├── Borders/          # Border and outline utilities
│   │   ├── FlexBoxAndGrid/   # Layout utilities
│   │   ├── Typography/       # Text and font utilities
│   │   └── ...
│   └── Variants/             # Variant system
└── TryMonorail/              # Blazor demo application

test/
├── MonorailCss.Tests/        # Unit tests
└── Benchmarks/               # Performance benchmarks
```

## Common Development Patterns

### Adding a New Plugin
1. Inherit from appropriate base class (`BaseUtilityNamespacePlugin`, etc.)
2. Implement required methods (`GetAllRules`, `Process`)
3. Plugin will be auto-discovered by `PluginManager`

### Extending the Design System
```csharp
var customDesignSystem = DesignSystem.Default with
{
    Colors = DesignSystem.Default.Colors.Add("brand", brandColors)
};
```

### Working with Variants
- Variants are processed by `VariantProcessor`
- Media queries handled separately from other modifiers
- Selector generation handled by `SelectorGenerator`

## Technical Notes

- **Target Framework**: .NET 9.0
- **Language Version**: C# 13.0
- **Nullable Reference Types**: Enabled
- **Platform Separation**: Build outputs separated by platform (Windows/Linux)
- **Embedded Resources**: CSS reset file embedded in assembly
- **Code Analysis**: StyleCop analyzers enabled