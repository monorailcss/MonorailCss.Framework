using System.Collections.Immutable;
using MonorailCss.Theme;
using Shouldly;

namespace MonorailCss.Tests;

public class CssFrameworkSettingsTests
{
    [Fact]
    public void CssFrameworkSettings_Should_Initialize_With_Defaults()
    {
        var settings = new CssFrameworkSettings();

        settings.Theme.ShouldNotBeNull();
        settings.Variants.ShouldBeEmpty();
        settings.Important.ShouldBe(false);
        settings.IncludePreflight.ShouldBe(true);
        settings.Applies.ShouldBeEmpty();
    }

    [Fact]
    public void CssFrameworkSettings_Should_Support_Init_Properties()
    {
        var theme = new MonorailCss.Theme.Theme();
        var variants = ImmutableHashSet.Create("hover", "focus", "active");
        var applies = ImmutableDictionary<string, string>.Empty.Add(".btn", "px-4 py-2");

        var settings = new CssFrameworkSettings
        {
            Theme = theme,
            Variants = variants,
            Important = true,
            IncludePreflight = false,
            Applies = applies
        };

        settings.Theme.ShouldBe(theme);
        settings.Variants.Count.ShouldBe(3);
        settings.Variants.ShouldContain("hover");
        settings.Important.ShouldBe(true);
        settings.IncludePreflight.ShouldBe(false);
        settings.Applies.Count.ShouldBe(1);
    }


    [Fact]
    public void Theme_Should_Initialize_With_Defaults()
    {
        var theme = new MonorailCss.Theme.Theme();

        // Theme now has defaults
        theme.Values.ShouldNotBeEmpty();
        theme.Values.Count.ShouldBeGreaterThan(300); // We have 369 defaults
        theme.Prefix.ShouldBe(string.Empty);

        // Check some defaults are present
        theme.ContainsKey("--color-red-500").ShouldBeTrue();
        theme.ContainsKey("--spacing").ShouldBeTrue();
    }

    [Fact]
    public void Theme_Should_Add_Values()
    {
        var theme = new MonorailCss.Theme.Theme();
        var initialCount = theme.Values.Count;

        theme = theme.Add("--custom-color", "#ff0000");

        theme.Values.Count.ShouldBe(initialCount + 1);
        theme.ContainsKey("--custom-color").ShouldBe(true);

        var value = theme.ResolveValue("--custom-color", []);
        value.ShouldNotBeNull();
        value.ShouldBe("#ff0000");
    }

    [Fact]
    public void Theme_Should_Resolve_Values()
    {
        var theme = new MonorailCss.Theme.Theme();
        theme = theme.Add("--color-red-500", "#ef4444");

        // Resolve should return CSS variable
        var resolved = theme.Resolve("red-500", ["--color"]);
        resolved.ShouldBe("var(--color-red-500)");

        // ResolveValue should return raw value
        var rawValue = theme.ResolveValue("red-500", ["--color"]);
        rawValue.ShouldBe("#ef4444");
    }

    [Fact]
    public void Theme_Should_Track_Usage()
    {
        var theme = new MonorailCss.Theme.Theme();
        theme = theme.Add("--color-red-500", "#ef4444");
        var tracker = new ThemeUsageTracker(theme);

        tracker.IsUsed("--color-red-500").ShouldBe(false);

        // Resolve marks as used
        tracker.Resolve("red-500", ["--color"]);

        tracker.IsUsed("--color-red-500").ShouldBe(true);
        tracker.GetUsedValues().ShouldContain("--color-red-500");
    }

    [Fact]
    public void Theme_Should_Be_Immutable()
    {
        var theme1 = new MonorailCss.Theme.Theme();
        var initialCount = theme1.Values.Count;
        var theme2 = theme1.Add("--custom-color", "#ff0000");

        theme1.Values.Count.ShouldBe(initialCount);
        theme2.Values.Count.ShouldBe(initialCount + 1);
        theme1.ShouldNotBe(theme2);
    }

    [Fact]
    public void Theme_Should_Support_Prefix()
    {
        var theme = new MonorailCss.Theme.Theme { Prefix = "tw" };

        theme.Prefix.ShouldBe("tw");

        // Test that prefix is applied in resolution
        theme = theme.Add("--color-red-500", "#ef4444");
        var resolved = theme.Resolve("red-500", ["--color"]);
        resolved.ShouldBe("var(--tw-color-red-500)");
    }

    [Fact]
    public void UtilityRegistry_Should_Auto_Discover_All_Utilities()
    {
        var utilityRegistry = new UtilityRegistry();

        // Should auto-discover all utilities in the assembly
        utilityRegistry.RegisteredUtilities.Count.ShouldBeGreaterThan(3); // More than just the original 3
    }

}