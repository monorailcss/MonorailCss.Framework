using Shouldly;

namespace MonorailCss.Tests.Integration;

public class MapColorPaletteIntegrationTest
{
    [Fact]
    public void MapColorPalette_ShouldTrackBothVariables_WhenUsedWithColorUtilities()
    {
        // Arrange - Create a theme with slate colors and map base to slate
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-slate-50", "#f8fafc")
            .Add("--color-slate-500", "#64748b")
            .Add("--color-slate-900", "#0f172a")
            .MapColorPalette("slate", "base");

        var settings = new CssFrameworkSettings { Theme = theme, IncludePreflight = false };
        var framework = new CssFramework(settings);

        // Act - Process color utilities using the mapped palette
        var result = framework.Process("bg-base-500 text-base-50");
        Console.WriteLine(result);

        // Assert - CSS should include both the mapped and underlying variables
        result.ShouldContain("--color-base-500: var(--color-slate-500)");
        result.ShouldContain("--color-slate-500: #64748b");
        result.ShouldContain("--color-base-50: var(--color-slate-50)");
        result.ShouldContain("--color-slate-50: #f8fafc");

        // And the utility classes should use the mapped variables
        result.ShouldContain(".bg-base-500");
        result.ShouldContain("background-color: var(--color-base-500)");
        result.ShouldContain(".text-base-50");
        result.ShouldContain("color: var(--color-base-50)");
    }

    [Fact]
    public void MapColorPalette_ShouldWorkWithOpacityModifiers()
    {
        // Arrange - Create a theme with slate colors and map base to slate
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-slate-500", "#64748b")
            .MapColorPalette("slate", "base");

        var settings = new CssFrameworkSettings { Theme = theme, IncludePreflight = false };
        var framework = new CssFramework(settings);

        // Act - Process color utility with opacity modifier
        var result = framework.Process("bg-base-500/50");
        Console.WriteLine(result);

        // Assert - Both variables should be tracked
        result.ShouldContain("--color-base-500: var(--color-slate-500)");
        result.ShouldContain("--color-slate-500: #64748b");

        // The utility should use color-mix with the mapped variable
        result.ShouldContain(".bg-base-500\\/50");
        result.ShouldContain("background-color: color-mix(in oklab, var(--color-base-500) 50%, transparent)");
    }

    [Fact]
    public void MapColorPalette_ShouldHandleChainedMappings()
    {
        // Arrange - Create a theme with chained mappings: primary -> base -> slate
        var theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-slate-500", "#64748b")
            .MapColorPalette("slate", "base")
            .MapColorPalette("base", "primary");

        var settings = new CssFrameworkSettings { Theme = theme, IncludePreflight = false };
        var framework = new CssFramework(settings);

        // Act - Process color utility using the chained mapping
        var result = framework.Process("bg-primary-500");
        Console.WriteLine(result);

        // Assert - All variables in the chain should be tracked
        result.ShouldContain("--color-primary-500: var(--color-base-500)");
        result.ShouldContain("--color-base-500: var(--color-slate-500)");
        result.ShouldContain("--color-slate-500: #64748b");

        // The utility should use the primary variable
        result.ShouldContain(".bg-primary-500");
        result.ShouldContain("background-color: var(--color-primary-500)");
    }
}