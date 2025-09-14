using System.Collections.Immutable;
using MonorailCss.Css;
using Shouldly;

namespace MonorailCss.Tests.Css;

public class PreflightTests
{
    [Fact]
    public void Process_WithDefaultTheme_ShouldIncludeDefaultFonts()
    {
        // Arrange
        var theme = new MonorailCss.Theme.Theme();

        // Act
        var result = PreflightCss.Process(theme);

        // Assert - now using CSS variables
        result.ShouldContain("font-family: var(--default-font-family,");
        result.ShouldContain("font-family: var(--default-mono-font-family,");
        result.ShouldContain("font-feature-settings: var(--default-font-feature-settings, normal)");
        result.ShouldContain("font-variation-settings: var(--default-font-variation-settings, normal)");
        result.ShouldNotContain("{{FONT_SANS}}");
        result.ShouldNotContain("{{FONT_MONO}}");
    }

    [Fact]
    public void Process_WithCustomFonts_ShouldUseCustomValues()
    {
        // Arrange
        var customValues = ImmutableDictionary.CreateBuilder<string, string>();
        customValues.Add("--font-sans", "Inter, sans-serif");
        customValues.Add("--font-mono", "JetBrains Mono, monospace");
        customValues.Add("--default-font-feature-settings", "'liga' 1");
        customValues.Add("--default-font-variation-settings", "'wght' 400");

        var theme = MonorailCss.Theme.Theme.CreateWithDefaults(customValues.ToImmutable());

        // Act
        var result = PreflightCss.Process(theme);

        // Assert - the fallback values should use custom fonts
        result.ShouldContain("var(--default-font-family, Inter, sans-serif)");
        result.ShouldContain("var(--default-mono-font-family, JetBrains Mono, monospace)");
        result.ShouldContain("var(--default-font-feature-settings, 'liga' 1)");
        result.ShouldContain("var(--default-font-variation-settings, 'wght' 400)");
        result.ShouldNotContain("{{FONT_SANS}}");
        result.ShouldNotContain("{{FONT_MONO}}");
    }

    [Fact]
    public void CssFramework_WithPreflightEnabled_ShouldIncludePreflight()
    {
        // Arrange
        var settings = new CssFrameworkSettings { IncludePreflight = true };
        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("text-red-500");

        // Assert
        result.ShouldContain("@layer base");
        result.ShouldContain("box-sizing: border-box");
        result.ShouldContain("margin: 0");
        result.ShouldContain("padding: 0");
    }

    [Fact]
    public void CssFramework_WithPreflightDisabled_ShouldNotIncludePreflight()
    {
        // Arrange
        var settings = new CssFrameworkSettings { IncludePreflight = false };
        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("text-red-500");

        // Assert
        result.ShouldNotContain("@layer base");
        result.ShouldNotContain("box-sizing: border-box");
        result.ShouldContain("@layer utilities");
        result.ShouldContain("color:");
    }

    [Fact]
    public void CssFramework_PreflightInBaseLayer_ShouldBeFirstInLayer()
    {
        // Arrange
        var settings = new CssFrameworkSettings { IncludePreflight = true };
        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("text-red-500");

        // Assert
        result.ShouldContain("@layer base");

        var baseLayerIndex = result.IndexOf("@layer base", StringComparison.Ordinal);
        var baseLayerContent = result[baseLayerIndex..];
        var firstRuleIndex = baseLayerContent.IndexOf('{', baseLayerContent.IndexOf('{') + 1);
        var firstRule = baseLayerContent[firstRuleIndex..];

        // The first rule after @layer base { should be from preflight
        firstRule.ShouldContain("box-sizing");
    }

    [Fact]
    public void Preflight_ShouldIncludeAllEssentialResets()
    {
        // Arrange
        var theme = new MonorailCss.Theme.Theme();

        // Act
        var result = PreflightCss.Process(theme);

        // Assert
        // Box model resets
        result.ShouldContain("box-sizing: border-box");
        result.ShouldContain("margin: 0");
        result.ShouldContain("padding: 0");
        result.ShouldContain("border: 0 solid");

        // Typography resets
        result.ShouldContain("line-height: 1.5");
        result.ShouldContain("font-size: inherit");
        result.ShouldContain("font-weight: inherit");

        // List resets
        result.ShouldContain("list-style: none");

        // Form element resets
        result.ShouldContain("font: inherit");
        result.ShouldContain("color: inherit");

        // Image/video constraints
        result.ShouldContain("max-width: 100%");
        result.ShouldContain("height: auto");

        // Textarea behavior
        result.ShouldContain("resize: vertical");
    }
}