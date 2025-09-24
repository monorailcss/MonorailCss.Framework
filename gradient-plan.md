# Gradient Position Utilities Implementation Plan

## Problem Summary

The CanonicalUtilitiesTest has 64 failing tests related to gradient position utilities. These utilities control where gradient color stops are positioned along the gradient axis.

### Failing Utilities
- `from-0%` through `from-100%` (21 utilities, 5% increments)
- `to-0%` through `to-100%` (21 utilities, 5% increments)
- `via-0%` through `via-100%` (21 utilities, 5% increments)
- Plus `from-100%` (1 additional)

Total: 64 utilities

## Current State

### What Exists
- `GradientFromUtility`, `GradientToUtility`, `GradientViaUtility` - Handle gradient COLORS (e.g., `from-blue-500`)
- PropertyRegistrationStage has @property declarations for:
  - `--tw-gradient-from-position` (syntax: `<length-percentage>`, initial: `0%`)
  - `--tw-gradient-to-position` (syntax: `<length-percentage>`, initial: `100%`)
  - `--tw-gradient-via-position` (syntax: `<length-percentage>`, initial: `50%`)

### What's Missing
- Utilities to set the gradient position custom properties
- These are separate from color utilities and control stop positions

## Implementation Plan

### 1. Create Three New Utility Classes

#### File: `src/MonorailCss/Utilities/Backgrounds/GradientFromPositionUtility.cs`

```csharp
using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Handles gradient from position utilities (from-0% through from-100%).
/// Sets the starting position for gradient color stops.
/// CSS: --tw-gradient-from-position: value
/// </summary>
internal class GradientFromPositionUtility : BaseStaticUtility
{
    public override ImmutableDictionary<string, (string property, string value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string, string)>
        {
            ["from-0%"] = ("--tw-gradient-from-position", "0%"),
            ["from-5%"] = ("--tw-gradient-from-position", "5%"),
            ["from-10%"] = ("--tw-gradient-from-position", "10%"),
            ["from-15%"] = ("--tw-gradient-from-position", "15%"),
            ["from-20%"] = ("--tw-gradient-from-position", "20%"),
            ["from-25%"] = ("--tw-gradient-from-position", "25%"),
            ["from-30%"] = ("--tw-gradient-from-position", "30%"),
            ["from-35%"] = ("--tw-gradient-from-position", "35%"),
            ["from-40%"] = ("--tw-gradient-from-position", "40%"),
            ["from-45%"] = ("--tw-gradient-from-position", "45%"),
            ["from-50%"] = ("--tw-gradient-from-position", "50%"),
            ["from-55%"] = ("--tw-gradient-from-position", "55%"),
            ["from-60%"] = ("--tw-gradient-from-position", "60%"),
            ["from-65%"] = ("--tw-gradient-from-position", "65%"),
            ["from-70%"] = ("--tw-gradient-from-position", "70%"),
            ["from-75%"] = ("--tw-gradient-from-position", "75%"),
            ["from-80%"] = ("--tw-gradient-from-position", "80%"),
            ["from-85%"] = ("--tw-gradient-from-position", "85%"),
            ["from-90%"] = ("--tw-gradient-from-position", "90%"),
            ["from-95%"] = ("--tw-gradient-from-position", "95%"),
            ["from-100%"] = ("--tw-gradient-from-position", "100%"),
        });
}
```

#### File: `src/MonorailCss/Utilities/Backgrounds/GradientToPositionUtility.cs`

```csharp
using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Handles gradient to position utilities (to-0% through to-100%).
/// Sets the ending position for gradient color stops.
/// CSS: --tw-gradient-to-position: value
/// </summary>
internal class GradientToPositionUtility : BaseStaticUtility
{
    public override ImmutableDictionary<string, (string property, string value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string, string)>
        {
            ["to-0%"] = ("--tw-gradient-to-position", "0%"),
            ["to-5%"] = ("--tw-gradient-to-position", "5%"),
            ["to-10%"] = ("--tw-gradient-to-position", "10%"),
            ["to-15%"] = ("--tw-gradient-to-position", "15%"),
            ["to-20%"] = ("--tw-gradient-to-position", "20%"),
            ["to-25%"] = ("--tw-gradient-to-position", "25%"),
            ["to-30%"] = ("--tw-gradient-to-position", "30%"),
            ["to-35%"] = ("--tw-gradient-to-position", "35%"),
            ["to-40%"] = ("--tw-gradient-to-position", "40%"),
            ["to-45%"] = ("--tw-gradient-to-position", "45%"),
            ["to-50%"] = ("--tw-gradient-to-position", "50%"),
            ["to-55%"] = ("--tw-gradient-to-position", "55%"),
            ["to-60%"] = ("--tw-gradient-to-position", "60%"),
            ["to-65%"] = ("--tw-gradient-to-position", "65%"),
            ["to-70%"] = ("--tw-gradient-to-position", "70%"),
            ["to-75%"] = ("--tw-gradient-to-position", "75%"),
            ["to-80%"] = ("--tw-gradient-to-position", "80%"),
            ["to-85%"] = ("--tw-gradient-to-position", "85%"),
            ["to-90%"] = ("--tw-gradient-to-position", "90%"),
            ["to-95%"] = ("--tw-gradient-to-position", "95%"),
            ["to-100%"] = ("--tw-gradient-to-position", "100%"),
        });
}
```

#### File: `src/MonorailCss/Utilities/Backgrounds/GradientViaPositionUtility.cs`

```csharp
using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Handles gradient via position utilities (via-0% through via-100%).
/// Sets the middle position for gradient color stops.
/// CSS: --tw-gradient-via-position: value
/// </summary>
internal class GradientViaPositionUtility : BaseStaticUtility
{
    public override ImmutableDictionary<string, (string property, string value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string, string)>
        {
            ["via-0%"] = ("--tw-gradient-via-position", "0%"),
            ["via-5%"] = ("--tw-gradient-via-position", "5%"),
            ["via-10%"] = ("--tw-gradient-via-position", "10%"),
            ["via-15%"] = ("--tw-gradient-via-position", "15%"),
            ["via-20%"] = ("--tw-gradient-via-position", "20%"),
            ["via-25%"] = ("--tw-gradient-via-position", "25%"),
            ["via-30%"] = ("--tw-gradient-via-position", "30%"),
            ["via-35%"] = ("--tw-gradient-via-position", "35%"),
            ["via-40%"] = ("--tw-gradient-via-position", "40%"),
            ["via-45%"] = ("--tw-gradient-via-position", "45%"),
            ["via-50%"] = ("--tw-gradient-via-position", "50%"),
            ["via-55%"] = ("--tw-gradient-via-position", "55%"),
            ["via-60%"] = ("--tw-gradient-via-position", "60%"),
            ["via-65%"] = ("--tw-gradient-via-position", "65%"),
            ["via-70%"] = ("--tw-gradient-via-position", "70%"),
            ["via-75%"] = ("--tw-gradient-via-position", "75%"),
            ["via-80%"] = ("--tw-gradient-via-position", "80%"),
            ["via-85%"] = ("--tw-gradient-via-position", "85%"),
            ["via-90%"] = ("--tw-gradient-via-position", "90%"),
            ["via-95%"] = ("--tw-gradient-via-position", "95%"),
            ["via-100%"] = ("--tw-gradient-via-position", "100%"),
        });
}
```

### 2. Key Implementation Details

#### Base Class Choice
- Use `BaseStaticUtility` since these are fixed mappings
- No theme integration needed
- No arbitrary value support needed (percentages are predefined)

#### Priority
- Inherits `UtilityPriority.ExactStatic` (0) from base class
- Ensures these are evaluated before functional utilities

#### Registration
- Automatic via reflection in `UtilityDiscovery.DiscoverAllUtilities()`
- No manual registration needed

### 3. Expected Behavior

#### CSS Output
When processing `from-50%`, the utility should generate:
```css
.from-50\% {
    --tw-gradient-from-position: 50%;
}
```

#### @property Declaration
The PropertyRegistrationStage already registers:
```css
@property --tw-gradient-from-position {
    syntax: "<length-percentage>";
    inherits: false;
    initial-value: 0%;
}
```

### 4. Testing Strategy

#### Existing Tests
The CanonicalUtilitiesTest already includes test data for these utilities:
- Expected CSS: `--tw-gradient-from-position: 50%`
- Expected @property with syntax `<length-percentage>`

#### Verification Steps
1. Build the project: `dotnet build`
2. Run the canonical tests: `dotnet test --filter "FullyQualifiedName~CanonicalUtilitiesTest"`
3. Verify all 64 gradient position tests pass
4. Test with interactive tool: `dotnet run --project tests/MonorailCss.Interactive -- from-50%`

### 5. Integration Points

#### Works With
- Gradient color utilities (`from-blue-500`, `to-red-500`, `via-green-500`)
- Gradient direction utilities (`bg-gradient-to-r`, `bg-gradient-to-t`)
- Background utilities that apply gradients

#### Example Usage
```html
<div class="bg-gradient-to-r from-blue-500 from-25% via-purple-500 via-50% to-pink-500 to-75%">
    <!-- Gradient with custom color stop positions -->
</div>
```

### 6. Implementation Checklist

- [ ] Create `GradientFromPositionUtility.cs`
- [ ] Create `GradientToPositionUtility.cs`
- [ ] Create `GradientViaPositionUtility.cs`
- [ ] Build project to verify no compilation errors
- [ ] Run canonical tests to verify all 64 failures are resolved
- [ ] Test with interactive tool for manual verification
- [ ] Commit changes when all tests pass

## Alternative Approaches Considered

### Dynamic Pattern Matching
Could use a functional utility with pattern matching for `from-(\d+)%` but:
- More complex implementation
- Lower priority than static mappings
- Tailwind v4 uses predefined percentages only

### Single Combined Utility
Could handle all three types in one utility but:
- Violates single responsibility principle
- Makes testing more complex
- Harder to maintain

## Conclusion

This implementation provides a straightforward solution that:
1. Matches Tailwind v4 behavior exactly
2. Integrates seamlessly with existing gradient utilities
3. Follows MonorailCSS architecture patterns
4. Resolves all 64 failing tests

The static utility approach is optimal for these fixed percentage values and ensures high performance with the highest priority evaluation.