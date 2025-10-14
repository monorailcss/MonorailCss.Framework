---
title: Architecture
description: Understand the internal architecture of MonorailCSS
order: 4
category: Advanced
tags: [architecture, internals]
---

MonorailCSS is built with a modular architecture designed for extensibility and performance.

## Core Components

### 1. CssFramework

The main entry point that orchestrates the entire processing pipeline. It handles parsing, compilation, post-processing, and CSS generation.

### 2. Utilities System

Self-contained components that compile CSS class candidates into AST nodes. Each utility handles specific CSS properties:

- **Static utilities** extend `BaseStaticUtility` for fixed property/value mappings
- **Functional utilities** extend `BaseFunctionalUtility` for theme-aware dynamic values
- **Specialized base classes** exist for common patterns (colors, spacing, filters)

### 3. Candidate System

Represents parsed utility classes with variants and modifiers. The `CandidateParser` tokenizes and structures raw class names.

### 4. AST (Abstract Syntax Tree)

Intermediate representation of CSS rules before final generation. Node types include:

- **Declaration** - CSS property-value pairs with optional importance flag
- **StyleRule** - CSS rules with selectors and nested declarations
- **NestedRule** - Nested rules using parent selector (`&`) syntax
- **AtRule** - At-rules like `@media`, `@supports`, `@layer`
- **ComponentRule** - Component-level style rules
- **Comment** - CSS comments
- **Context** - Container for nodes with associated metadata
- **RawCss** - Raw CSS content pass-through

### 5. Processing Pipeline

Multi-stage transformation pipeline that processes AST nodes through the following stages (in order):

1. **Theme variable tracking** - Tracks which theme variables are used for optimization
2. **Arbitrary value validation** - Validates user-defined arbitrary values in square brackets
3. **Negative value normalization** - Handles negative value prefixes (e.g., `-m-4`)
4. **Color modifier processing** - Applies color opacity and mixing modifiers
5. **Important flag handling** - Processes `!` important modifiers
6. **Variable fallback resolution** - Resolves CSS variable fallback values
7. **Property registration** - Registers CSS custom properties for tracking
8. **Processing and sorting** - Applies variants and sorts rules for proper CSS output order
9. **Declaration merging** - Combines duplicate declarations within rules
10. **Media query consolidation** - Groups rules by media queries
11. **Layer assignment** - Assigns CSS cascade layers (base, components, utilities)

### 6. Theme System

Manages design tokens and CSS custom properties. Supports theme customization and usage tracking for optimization.

### 7. Variant System

Handles pseudo-classes, media queries, and other CSS modifiers. Built-in variants are automatically registered.


### 8. Apply/Component System

Processes `@apply` directives for creating reusable component classes. The `ApplyProcessor` expands utility classes into CSS declarations for custom selectors.

Key features:
- Dedicated processing pipeline for apply rules
- Supports variants within component classes (e.g., `hover:bg-blue-500` in applies)
- Handles selector transformation for component styles
- Merges declarations and properly wraps with media queries/at-rules

### 9. Support Components

Additional components that support core functionality:

- **CssPropertyRegistry** - Tracks and manages CSS custom properties used throughout the generated stylesheet
- **ThemeUsageTracker** - Monitors which theme variables are referenced, enabling tree-shaking of unused theme values for optimization
- **PostProcessor** - Handles variant application and wraps rules with appropriate selectors and at-rules
- **SortingManager** - Ensures proper CSS output ordering based on utility priorities and variant weights
- **DeclarationMerger** - Combines duplicate CSS declarations within rules to minimize output size

## Extensibility

### Runtime Customization

The framework supports adding custom utilities and variants at runtime:

- **AddUtility(IUtility)** - Register a single custom utility implementation
- **AddUtilities(IEnumerable<IUtility>)** - Register multiple custom utilities at once
- **AddVariant(IVariant, bool)** - Register custom variants with optional overwrite of built-in variants
- **CustomUtilityFactory** - Creates utilities from configuration settings

This allows for framework extension without modifying core code. Custom utilities/variants can be added through settings or programmatically.

## Additional Features

### Preflight CSS

Optional CSS reset/normalize generation via the `IncludePreflight` setting. When enabled, the framework generates Tailwind-compatible preflight styles that provide consistent cross-browser defaults.

### Font Variable Handling

Special handling for font family variables to maintain Tailwind v4 compatibility:
- Automatic inclusion of `--font-sans` and `--font-mono` variables
- Default fallback values for common font stacks
- `--default-font-family` and `--default-mono-font-family` derived variables

### Layer System

Utilities are assigned to CSS cascade layers for proper specificity control:
- **base** - Reset and base styles (preflight)
- **components** - Component classes from `@apply` directives
- **utilities** - Utility classes

This ensures utilities can always override components, and components can override base styles.

## Key Design Patterns

- **Auto-Discovery**: Utilities are automatically discovered via reflection at startup
- **Priority System**: Utilities have priorities (0-1000) determining evaluation order
- **Immutable Data**: Extensive use of immutable collections
- **Pipeline Architecture**: Modular stages for processing transformations
- **Theme Resolution**: Values resolved through namespace chains with fallback support
- **Arbitrary Values**: Support for user-defined values in square brackets `[value]`

## Testing Strategy

The project uses integration testing rather than unit testing individual utilities. The main test file `tests/MonorailCss.Tests/Interactive/AllUtilitiesIntegrationTest.cs` verifies that utilities generate expected CSS properties.

When adding new utilities:
1. Create the utility class in appropriate category folder
2. Add test cases to `AllUtilitiesIntegrationTest.cs`
3. Verify all tests pass before committing
