# MonorailCss Utilities Architecture Guide

## Overview
This document outlines MonorailCss's utility implementation patterns, discovered through analysis of the existing codebase. Use this as a reference when implementing new utilities to maintain consistency with the framework's architecture.

## Utility Implementation Patterns

### 1. Static Utilities (`BaseStaticUtility`)
**Used for:** Fixed CSS properties with predefined values  
**Pattern:** Simple mapping of class name to CSS property/value  
**Example:** `DisplayUtility` for `block`, `flex`, `grid`, etc.

**Key characteristics:**
- Extends `BaseStaticUtility`
- Override `StaticValues` property with `ImmutableDictionary<string, (string property, string value)>`
- Priority: `UtilityPriority.ExactStatic` (0 - highest priority)
- No theme integration needed
- No GetNamespaces() override needed (returns empty array)

```csharp
public class DisplayUtility : BaseStaticUtility
{
    public override ImmutableDictionary<string, (string property, string value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string, string)>
        {
            ["block"] = ("display", "block"),
            ["flex"] = ("display", "flex"),
            ["grid"] = ("display", "grid")
        });
}
```

### 2. Functional Utilities (`BaseFunctionalUtility`)
**Used for:** Theme-aware utilities with dynamic values  
**Pattern:** Resolves values from theme namespaces or handles bare values  
**Example:** `OpacityUtility` for `opacity-50`, `opacity-[0.75]`

**Key characteristics:**
- Extends `BaseFunctionalUtility`
- Override `Patterns` (utility prefixes like `["opacity"]`)
- Override `ThemeKeys` (theme namespaces like `["--opacity"]`)
- Override `HandleBareValue()` for bare value conversion
- Override `GenerateDeclarations()` for CSS output
- Priority: `UtilityPriority.NamespaceHandler` (400)
- Automatic theme resolution and arbitrary value support

```csharp
public class OpacityUtility : BaseFunctionalUtility
{
    public override string[] Patterns => ["opacity"];
    public override string[] ThemeKeys => ["--opacity"];
    
    public override string? HandleBareValue(string value, IReadOnlyDesignSystem designSystem) =>
        double.TryParse(value, out var num) && num >= 0 && num <= 1 ? value : null;

    public override IEnumerable<CssDeclaration> GenerateDeclarations(UtilityClass utilityClass, 
        IReadOnlyDesignSystem designSystem)
    {
        yield return new CssDeclaration("opacity", utilityClass.ResolvedValue);
    }
}
```

### 3. Specialized Base Classes

**Color Utilities:** `BaseColorUtility` - Handles color resolution with opacity modifiers  
**Spacing Utilities:** `BaseSpacingUtility` - Handles spacing with negative variants  
**Filter Utilities:** `BaseFilterUtility` - Complex filter function generation

### 4. Direct IUtility Implementation
**Used for:** Complex utilities requiring custom logic  
**Example:** `ContainerUtility` with responsive breakpoints

**Key characteristics:**
- Implements `IUtility` directly
- Full control over compilation logic
- Custom priority assignment
- Manual theme integration

## Architecture & Registration

### Auto-Discovery System
- `UtilityDiscovery.DiscoverAllUtilities()` uses reflection
- Finds all concrete classes implementing `IUtility`
- Requires parameterless constructor
- Automatic registration via `DesignSystem.RegisterAllUtilities()`

### Priority System
```csharp
ExactStatic = 0,         // Static mappings (highest priority)
ConstrainedFunctional = 100,  // Limited functional utilities  
NegativeVariant = 200,   // Negative variants
StandardFunctional = 300,  // Standard functional utilities
NamespaceHandler = 400,  // Theme namespace handlers
ArbitraryHandler = 500,  // Arbitrary value handlers
Fallback = 1000         // Fallback utilities
```

### Theme Integration
- Theme variables always prefixed with `--` (e.g., `--spacing`, `--color`)
- `MarkUsed()` to track which variables are output
- Automatic `var()` wrapping for CSS variable references
- Support for arbitrary values `[arbitrary-value]`
- Namespace chains for value resolution

## Testing Requirements

### Integration Testing
- **Primary test:** `AllUtilitiesIntegrationTest.cs` 
- Uses `[Theory]` with `[MemberData(nameof(AllUtilitiesTestData))]`
- Tests framework-level behavior (not unit testing individual utilities)
- Pattern: `[className, expectedCssProperty]`
- Example: `["opacity-50", "opacity: 0.5"]`

### Test Structure
```csharp
[Theory]
[MemberData(nameof(AllUtilitiesTestData))]
public void AllUtilities_ShouldGenerateAtLeastOneProperty(string className, string expectedProperty)
{
    var result = _cssFramework.Process(className);
    result.ShouldContain(expectedProperty);
}
```

### Test Data Organization
- Group by utility category (Layout, Typography, etc.)
- Include representative examples of each utility
- Test both basic and complex cases
- Verify theme integration where applicable

## Architectural Constraints & Conventions

### File Organization
- Utilities in `MonorailCss/src/MonorailCss/Utilities/`
- Organized by category folders (Layout, Typography, Effects, etc.)
- Base classes in `Utilities/Base/`
- Naming: `{UtilityName}Utility.cs`

### Implementation Rules
1. **Single Responsibility:** Each utility handles one CSS concept
2. **Theme First:** Use theme variables when possible
3. **Arbitrary Support:** Handle `[arbitrary]` values appropriately
4. **Negative Variants:** Support `-` prefix for applicable utilities
5. **Priority Assignment:** Choose appropriate priority level
6. **Documentation:** Include XML doc comments with examples

### Extension Points
- Custom base classes for utility families
- Theme namespace customization
- Value resolution overrides
- Custom validation logic
- Property registry integration

### CSS Output
- Modern CSS features (color-mix, logical properties)
- CSS custom properties for theme values
- Proper vendor prefixing where needed
- Canonical property ordering via sorting system

## Implementation Workflow

1. **Choose Pattern:** Static, Functional, or Custom IUtility
2. **Create Utility Class:** Following naming and organization conventions
3. **Add Theme Support:** If needed, define theme namespaces
4. **Add Test Cases:** Update `AllUtilitiesIntegrationTest.cs`
5. **Verify Build:** `dotnet build MonorailCss/MonorailCss.sln`
6. **Run Tests:** `dotnet test MonorailCss/MonorailCss.sln`
7. **Commit Changes:** When tests pass 100%

This architecture guide ensures consistency and maintainability when extending MonorailCss with new utility classes.