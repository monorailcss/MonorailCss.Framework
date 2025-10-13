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

Location: `src/MonorailCss/CssFramework.cs`

### 2. Utilities System

Self-contained components that compile CSS class candidates into AST nodes. Each utility handles specific CSS properties:

- **Static utilities** extend `BaseStaticUtility` for fixed property/value mappings
- **Functional utilities** extend `BaseFunctionalUtility` for theme-aware dynamic values
- **Specialized base classes** exist for common patterns (colors, spacing, filters)

Location: `src/MonorailCss/Utilities/`

### 3. Candidate System

Represents parsed utility classes with variants and modifiers. The `CandidateParser` tokenizes and structures raw class names.

Location: `src/MonorailCss/Candidates/`

### 4. AST (Abstract Syntax Tree)

Intermediate representation of CSS rules before final generation. Includes `Declaration`, `Rule`, `ComponentRule` nodes.

Location: `src/MonorailCss/Ast/`

### 5. Processing Pipeline

Multi-stage transformation pipeline that processes AST nodes through:

- Theme variable tracking
- Arbitrary value validation
- Negative value normalization
- Color modifier processing
- Important flag handling
- Variable fallback resolution
- Declaration merging
- Media query consolidation
- Layer assignment

Location: `src/MonorailCss/Pipeline/`

### 6. Theme System

Manages design tokens and CSS custom properties. Supports theme customization and usage tracking for optimization.

Location: `src/MonorailCss/Theme/`

### 7. Variant System

Handles pseudo-classes, media queries, and other CSS modifiers. Built-in variants are automatically registered.

Location: `src/MonorailCss/Variants/`

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
