using MonorailCss.Theme;
using Shouldly;

namespace MonorailCss.Tests.Theme;

public class ThemeTests
{
    [Fact]
    public void Theme_ShouldStoreAndRetrieveValues()
    {
        // Arrange & Act
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444")
            .Add("--color-blue-500", "#3b82f6")
            .Add("--spacing-4", "1rem");

        // Assert
        theme.ResolveValue("--color-red-500", []).ShouldBe("#ef4444");
        theme.ResolveValue("--color-blue-500", []).ShouldBe("#3b82f6");
        theme.ResolveValue("--spacing-4", []).ShouldBe("1rem");
        theme.ResolveValue("--nonexistent", []).ShouldBeNull();
    }

    [Fact]
    public void Resolve_ShouldReturnCssVariable_WhenValueExists()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444")
            .Add("--background-color-primary", "#1a1a1a");

        // Act & Assert
        theme.Resolve("red-500", ["--color"]).ShouldBe("var(--color-red-500)");
        theme.Resolve("primary", ["--background-color"]).ShouldBe("var(--background-color-primary)");
    }

    [Fact]
    public void Resolve_ShouldTryMultipleNamespaces_InOrder()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444")
            .Add("--text-color-red-500", "#dc2626");

        // Act
        var result1 = theme.Resolve("red-500", ["--background-color", "--color"]);
        var result2 = theme.Resolve("red-500", ["--text-color", "--color"]);

        // Assert
        result1.ShouldBe("var(--color-red-500)"); // Falls back to --color
        result2.ShouldBe("var(--text-color-red-500)"); // Uses --text-color first
    }

    [Fact]
    public void Resolve_ShouldHandleNullValue_ByTryingNamespacesDirectly()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color", "#000000")
            .Add("--spacing", "1rem");

        // Act
        var result = theme.Resolve(null, ["--spacing", "--color"]);

        // Assert
        result.ShouldBe("var(--spacing)"); // First namespace that exists
    }

    [Fact]
    public void ResolveValue_ShouldReturnRawValue_NotCssVariable()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444")
            .Add("--spacing-4", "1rem");

        // Act & Assert
        theme.ResolveValue("red-500", ["--color"]).ShouldBe("#ef4444");
        theme.ResolveValue("4", ["--spacing"]).ShouldBe("1rem");
    }

    [Fact]
    public void Resolve_ShouldHandleLegacyDotToUnderscore_Conversion()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--spacing-2_5", "0.625rem"); // 2.5 stored as 2_5

        // Act
        var result = theme.Resolve("2.5", ["--spacing"]);

        // Assert
        result.ShouldBe("var(--spacing-2_5)");
    }

    [Fact]
    public void UsageTracking_ShouldTrackUsedValues()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444")
            .Add("--color-blue-500", "#3b82f6")
            .Add("--color-green-500", "#10b981");
        var tracker = new ThemeUsageTracker(theme);

        // Act
        tracker.Resolve("red-500", ["--color"]);
        tracker.ResolveValue("blue-500", ["--color"]);
        // green-500 is not used

        // Assert
        tracker.IsUsed("--color-red-500").ShouldBeTrue();
        tracker.IsUsed("--color-blue-500").ShouldBeTrue();
        tracker.IsUsed("--color-green-500").ShouldBeFalse();

        var usedValues = tracker.GetUsedValues().ToList();
        usedValues.ShouldContain("--color-red-500");
        usedValues.ShouldContain("--color-blue-500");
        usedValues.ShouldNotContain("--color-green-500");
    }

    [Fact]
    public void MarkUsed_ShouldManuallyMarkValueAsUsed()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444");
        var tracker = new ThemeUsageTracker(theme);

        // Act
        tracker.MarkUsed("--color-red-500");

        // Assert
        tracker.IsUsed("--color-red-500").ShouldBeTrue();
    }

    [Fact]
    public void KeysInNamespace_ShouldReturnAllKeysInNamespace()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444")
            .Add("--color-blue-500", "#3b82f6")
            .Add("--spacing-4", "1rem")
            .Add("--spacing-8", "2rem");

        // Act
        var colorKeys = theme.KeysInNamespace("--color");
        var spacingKeys = theme.KeysInNamespace("spacing"); // Without --

        // Assert
        colorKeys.Count.ShouldBe(2);
        colorKeys.ShouldContain("--color-red-500");
        colorKeys.ShouldContain("--color-blue-500");

        spacingKeys.Count.ShouldBe(2);
        spacingKeys.ShouldContain("--spacing-4");
        spacingKeys.ShouldContain("--spacing-8");
    }

    [Fact]
    public void Namespace_ShouldReturnAllKeyValuePairsInNamespace()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444")
            .Add("--color-blue-500", "#3b82f6")
            .Add("--spacing-4", "1rem");

        // Act
        var colorNamespace = theme.Namespace("--color");

        // Assert
        colorNamespace.Count.ShouldBe(2);
        colorNamespace["--color-red-500"].ShouldBe("#ef4444");
        colorNamespace["--color-blue-500"].ShouldBe("#3b82f6");
        colorNamespace.ContainsKey("--spacing-4").ShouldBeFalse();
    }

    [Fact]
    public void ClearNamespace_ShouldRemoveAllValuesFromNamespace()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444")
            .Add("--color-blue-500", "#3b82f6")
            .Add("--spacing-4", "1rem")
            .Add("--spacing-8", "2rem");

        // Act
        var newTheme = theme.ClearNamespace("--color");

        // Assert
        newTheme.ContainsKey("--color-red-500").ShouldBeFalse();
        newTheme.ContainsKey("--color-blue-500").ShouldBeFalse();
        newTheme.ContainsKey("--spacing-4").ShouldBeTrue();
        newTheme.ContainsKey("--spacing-8").ShouldBeTrue();

        // Original theme should be unchanged (immutable)
        theme.ContainsKey("--color-red-500").ShouldBeTrue();
    }

    [Fact]
    public void Theme_ShouldSupportPrefix()
    {
        // Arrange
        var theme = new MonorailCss.Theme.Theme(
            MonorailCss.Theme.Theme.CreateEmpty().Values)
        { Prefix = "tw" }
            .Add("--color-red-500", "#ef4444");

        // Act
        var result = theme.Resolve("red-500", ["--color"]);

        // Assert
        result.ShouldBe("var(--tw-color-red-500)");
    }

    [Fact]
    public void Theme_ShouldBeImmutable()
    {
        // Arrange
        var originalTheme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444");

        // Act
        var newTheme = originalTheme.Add("--color-blue-500", "#3b82f6");

        // Assert
        originalTheme.Values.Count.ShouldBe(1);
        originalTheme.ContainsKey("--color-blue-500").ShouldBeFalse();

        newTheme.Values.Count.ShouldBe(2);
        newTheme.ContainsKey("--color-red-500").ShouldBeTrue();
        newTheme.ContainsKey("--color-blue-500").ShouldBeTrue();
    }

    [Fact]
    public void Resolve_ShouldReturnNull_WhenValueNotFound()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty();

        // Act
        var result = theme.Resolve("nonexistent", ["--color"]);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ThemeNamespaces_ShouldWorkWithCommonTailwindNamespaces()
    {
        // Arrange - Set up common Tailwind theme structure
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            // Color namespace
            .Add("--color-red-500", "#ef4444")
            .Add("--color-blue-500", "#3b82f6")
            // Background color namespace (specific overrides)
            .Add("--background-color-primary", "#1a1a1a")
            // Text color namespace (specific overrides)
            .Add("--text-color-primary", "#ffffff")
            // Spacing namespace
            .Add("--spacing-0", "0px")
            .Add("--spacing-1", "0.25rem")
            .Add("--spacing-2", "0.5rem")
            .Add("--spacing-4", "1rem");

        // Act & Assert - Background colors should fallback to color
        theme.Resolve("red-500", ["--background-color", "--color"])
            .ShouldBe("var(--color-red-500)");
        theme.Resolve("primary", ["--background-color", "--color"])
            .ShouldBe("var(--background-color-primary)");

        // Text colors should fallback to color
        theme.Resolve("blue-500", ["--text-color", "--color"])
            .ShouldBe("var(--color-blue-500)");
        theme.Resolve("primary", ["--text-color", "--color"])
            .ShouldBe("var(--text-color-primary)");

        // Spacing should work independently
        theme.Resolve("4", ["--spacing"])
            .ShouldBe("var(--spacing-4)");
    }

    [Fact]
    public void MapColorPalette_ShouldCreateMappingsWithVarSyntax()
    {
        // Arrange - Create a theme with slate colors
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-slate-50", "#f8fafc")
            .Add("--color-slate-100", "#f1f5f9")
            .Add("--color-slate-200", "#e2e8f0")
            .Add("--color-slate-300", "#cbd5e1")
            .Add("--color-slate-400", "#94a3b8")
            .Add("--color-slate-500", "#64748b")
            .Add("--color-slate-600", "#475569")
            .Add("--color-slate-700", "#334155")
            .Add("--color-slate-800", "#1e293b")
            .Add("--color-slate-900", "#0f172a")
            .Add("--color-slate-950", "#020617");

        // Act - Map "base" to "slate"
        var mappedTheme = theme.MapColorPalette("slate", "base");

        // Assert - Mapped values should use var() syntax
        mappedTheme.ResolveValue("--color-base-50", []).ShouldBe("var(--color-slate-50)");
        mappedTheme.ResolveValue("--color-base-500", []).ShouldBe("var(--color-slate-500)");
        mappedTheme.ResolveValue("--color-base-950", []).ShouldBe("var(--color-slate-950)");
    }

    [Fact]
    public void MapColorPalette_ShouldResolveCorrectly_WhenUsedWithColorUtilities()
    {
        // Arrange - Create a theme with slate colors and map base to slate
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-slate-50", "#f8fafc")
            .Add("--color-slate-500", "#64748b")
            .Add("--color-slate-900", "#0f172a")
            .MapColorPalette("slate", "base");

        // Act - Resolve base colors through color namespace
        var resolved50 = theme.Resolve("base-50", ["--color"]);
        var resolved500 = theme.Resolve("base-500", ["--color"]);
        var resolved900 = theme.Resolve("base-900", ["--color"]);

        // Assert - Should resolve to var() references of base colors
        resolved50.ShouldBe("var(--color-base-50)");
        resolved500.ShouldBe("var(--color-base-500)");
        resolved900.ShouldBe("var(--color-base-900)");
    }

    [Fact]
    public void MapColorPalette_ShouldTrackBothMappedAndUnderlyingVariables()
    {
        // Arrange - Create a theme with slate colors and map base to slate
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-slate-50", "#f8fafc")
            .Add("--color-slate-500", "#64748b")
            .Add("--color-slate-900", "#0f172a")
            .MapColorPalette("slate", "base");
        var tracker = new ThemeUsageTracker(theme);

        // Act - Resolve base-500 which should track both base-500 and slate-500
        var resolved = tracker.Resolve("base-500", ["--color"]);
        resolved.ShouldNotBeEmpty();

        // Assert - The mapped variable should be tracked
        tracker.IsUsed("--color-base-500").ShouldBeTrue();

        // Note: The underlying variable tracking will be implemented when we fix the resolve method
        // For now, this test documents the expected behavior
    }

    [Fact]
    public void MapColorPalette_ShouldWorkWithMultipleMappings()
    {
        // Arrange - Create a theme with multiple color palettes
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-slate-500", "#64748b")
            .Add("--color-blue-500", "#3b82f6")
            .MapColorPalette("slate", "base")
            .MapColorPalette("blue", "primary");

        // Act
        var baseResolved = theme.Resolve("base-500", ["--color"]);
        var primaryResolved = theme.Resolve("primary-500", ["--color"]);

        // Assert
        baseResolved.ShouldBe("var(--color-base-500)");
        primaryResolved.ShouldBe("var(--color-primary-500)");

        // Verify the mapped values reference the correct underlying colors
        theme.ResolveValue("--color-base-500", []).ShouldBe("var(--color-slate-500)");
        theme.ResolveValue("--color-primary-500", []).ShouldBe("var(--color-blue-500)");
    }

    [Fact]
    public void MapColorPalette_ShouldHandleMissingShades()
    {
        // Arrange - Create a theme with only some slate shades
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-slate-500", "#64748b")
            .Add("--color-slate-700", "#334155")
            .MapColorPalette("slate", "base");

        // Act - Try to resolve mapped shades
        var resolved500 = theme.Resolve("base-500", ["--color"]);
        var resolved700 = theme.Resolve("base-700", ["--color"]);
        var resolved300 = theme.Resolve("base-300", ["--color"]); // This shade doesn't exist in slate

        // Assert
        resolved500.ShouldBe("var(--color-base-500)");
        resolved700.ShouldBe("var(--color-base-700)");
        resolved300.ShouldBe("var(--color-base-300)"); // Still creates mapping even if underlying doesn't exist

        // The mapped values should still reference the (potentially non-existent) underlying colors
        theme.ResolveValue("--color-base-500", []).ShouldBe("var(--color-slate-500)");
        theme.ResolveValue("--color-base-300", []).ShouldBe("var(--color-slate-300)"); // References non-existent color
    }

    [Fact]
    public void ResolveInlineValue_WithSimpleVariableReference_ResolvesCorrectly()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-brand-500", "oklch(0.72 0.11 178)")
            .Add("--font-inter", "\"Inter\", sans-serif");

        // Act
        var resolvedColor = theme.ResolveInlineValue("var(--color-brand-500)");
        var resolvedFont = theme.ResolveInlineValue("var(--font-inter)");

        // Assert
        resolvedColor.ShouldBe("oklch(0.72 0.11 178)");
        resolvedFont.ShouldBe("\"Inter\", sans-serif");
    }

    [Fact]
    public void ResolveInlineValue_WithNestedVariableReferences_ResolvesRecursively()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-brand-500", "oklch(0.72 0.11 178)")
            .Add("--color-primary", "var(--color-brand-500)");

        // Act
        var resolved = theme.ResolveInlineValue("var(--color-primary)");

        // Assert
        resolved.ShouldBe("oklch(0.72 0.11 178)");
    }

    [Fact]
    public void ResolveInlineValue_WithMultipleLevelsOfNesting_ResolvesAll()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--base-color", "#3b82f6")
            .Add("--brand-color", "var(--base-color)")
            .Add("--primary-color", "var(--brand-color)");

        // Act
        var resolved = theme.ResolveInlineValue("var(--primary-color)");

        // Assert
        resolved.ShouldBe("#3b82f6");
    }

    [Fact]
    public void ResolveInlineValue_WithFallbackValue_UsesFallbackWhenVariableNotFound()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-default", "#000000");

        // Act
        var resolved = theme.ResolveInlineValue("var(--color-nonexistent, #ff0000)");

        // Assert
        resolved.ShouldBe("#ff0000");
    }

    [Fact]
    public void ResolveInlineValue_WithNestedFallback_ResolvesCorrectly()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-brand-500", "#3b82f6");

        // Act
        var resolved = theme.ResolveInlineValue("var(--color-user-accent, var(--color-brand-500))");

        // Assert - Should use the nested fallback
        resolved.ShouldBe("#3b82f6");
    }

    [Fact]
    public void ResolveInlineValue_WithMultipleVariablesInValue_ResolvesAllReferences()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--spacing-x", "1rem")
            .Add("--spacing-y", "0.5rem");

        // Act
        var resolved = theme.ResolveInlineValue("var(--spacing-x) var(--spacing-y)");

        // Assert
        resolved.ShouldBe("1rem 0.5rem");
    }

    [Fact]
    public void ResolveInlineValue_WithNoVariableReferences_ReturnsOriginalValue()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty();

        // Act
        var resolved = theme.ResolveInlineValue("#3b82f6");

        // Assert
        resolved.ShouldBe("#3b82f6");
    }

    [Fact]
    public void ResolveInlineValue_WithNonexistentVariableAndNoFallback_LeavesVarReference()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty();

        // Act
        var resolved = theme.ResolveInlineValue("var(--color-nonexistent)");

        // Assert - Should leave the var() reference as-is since no fallback
        resolved.ShouldBe("var(--color-nonexistent)");
    }

    [Fact]
    public void ResolveInlineValue_WithMaxDepthExceeded_PreventInfiniteRecursion()
    {
        // Arrange - Create a circular reference
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-a", "var(--color-b)")
            .Add("--color-b", "var(--color-a)");

        // Act
        var resolved = theme.ResolveInlineValue("var(--color-a)", maxDepth: 3);

        // Assert - Should stop after max depth
        // After 3 iterations: var(--color-a) -> var(--color-b) -> var(--color-a) -> stops
        resolved.ShouldNotBeNull();
        resolved.ShouldContain("var("); // Should still contain a var() reference
    }

    [Fact]
    public void AddInline_WithVariableReference_ResolvesAndAdds()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--font-inter", "\"Inter\", sans-serif");

        // Act
        var updatedTheme = theme.AddInline("--font-sans", "var(--font-inter)");

        // Assert
        updatedTheme.ResolveValue("--font-sans", []).ShouldBe("\"Inter\", sans-serif");
    }

    [Fact]
    public void AddInline_WithNestedReferences_ResolvesAllLevels()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--base-color", "#3b82f6")
            .Add("--brand-color", "var(--base-color)");

        // Act
        var updatedTheme = theme.AddInline("--primary-color", "var(--brand-color)");

        // Assert
        updatedTheme.ResolveValue("--primary-color", []).ShouldBe("#3b82f6");
    }

    [Fact]
    public void AddInline_WithoutVariableReference_AddsDirectly()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty();

        // Act
        var updatedTheme = theme.AddInline("--color-primary", "#3b82f6");

        // Assert
        updatedTheme.ResolveValue("--color-primary", []).ShouldBe("#3b82f6");
    }
}