using System.Collections.Immutable;
using Shouldly;

namespace MonorailCss.Tests.Utilities;

public class ThemeCustomizationTests
{
    [Fact]
    public void AddColorPalette_ShouldAddAllShades()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty();
        var brandColors = ImmutableDictionary.CreateBuilder<string, string>();
        brandColors["50"] = "oklch(98.5% 0.012 280)";
        brandColors["100"] = "oklch(96.3% 0.024 280)";
        brandColors["200"] = "oklch(93.1% 0.048 280)";
        brandColors["300"] = "oklch(87.8% 0.096 280)";
        brandColors["400"] = "oklch(79.6% 0.152 280)";
        brandColors["500"] = "oklch(71.2% 0.191 280)";
        brandColors["600"] = "oklch(62.8% 0.208 280)";
        brandColors["700"] = "oklch(53.4% 0.189 280)";
        brandColors["800"] = "oklch(45.1% 0.156 280)";
        brandColors["900"] = "oklch(38.2% 0.127 280)";
        brandColors["950"] = "oklch(25.8% 0.086 280)";

        // Act
        var updatedTheme = theme.AddColorPalette("brand", brandColors.ToImmutable());

        // Assert
        updatedTheme.ResolveValue("--color-brand-50", []).ShouldBe("oklch(98.5% 0.012 280)");
        updatedTheme.ResolveValue("--color-brand-500", []).ShouldBe("oklch(71.2% 0.191 280)");
        updatedTheme.ResolveValue("--color-brand-950", []).ShouldBe("oklch(25.8% 0.086 280)");
    }

    [Fact]
    public void AddColorPalette_WithCustomShades_ShouldWork()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty();
        var customColors = ImmutableDictionary.CreateBuilder<string, string>();
        customColors["light"] = "#f0f0f0";
        customColors["DEFAULT"] = "#808080";
        customColors["dark"] = "#202020";

        // Act
        var updatedTheme = theme.AddColorPalette("custom", customColors.ToImmutable());

        // Assert
        updatedTheme.ResolveValue("--color-custom-light", []).ShouldBe("#f0f0f0");
        updatedTheme.ResolveValue("--color-custom-DEFAULT", []).ShouldBe("#808080");
        updatedTheme.ResolveValue("--color-custom-dark", []).ShouldBe("#202020");
    }

    [Fact]
    public void AddFontFamily_ShouldAddFontStack()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty();

        // Act
        var updatedTheme = theme.AddFontFamily("display", "'Playfair Display', serif");

        // Assert
        updatedTheme.ResolveValue("--font-display", []).ShouldBe("'Playfair Display', serif");
    }

    [Fact]
    public void AddFontFamily_Multiple_ShouldWork()
    {
        // Arrange
        var theme = MonorailCss.Theme.Theme.CreateEmpty();

        // Act
        var updatedTheme = theme
            .AddFontFamily("display", "'Playfair Display', serif")
            .AddFontFamily("body", "'Inter', sans-serif")
            .AddFontFamily("code", "'Fira Code', monospace");

        // Assert
        updatedTheme.ResolveValue("--font-display", []).ShouldBe("'Playfair Display', serif");
        updatedTheme.ResolveValue("--font-body", []).ShouldBe("'Inter', sans-serif");
        updatedTheme.ResolveValue("--font-code", []).ShouldBe("'Fira Code', monospace");
    }

    [Fact]
    public void Theme_ShouldMaintainImmutability()
    {
        // Arrange
        var originalTheme = MonorailCss.Theme.Theme.CreateEmpty();
        var colors = ImmutableDictionary.Create<string, string>()
            .Add("500", "#123456");

        // Act
        var modifiedTheme = originalTheme.AddColorPalette("test", colors);

        // Assert
        originalTheme.ResolveValue("--color-test-500", []).ShouldBeNull();
        modifiedTheme.ResolveValue("--color-test-500", []).ShouldBe("#123456");
    }

    [Fact]
    public void AddColorPalette_ToDefaultTheme_ShouldPreserveDefaults()
    {
        // Arrange
        var theme = new MonorailCss.Theme.Theme(); // Default theme with all Tailwind colors
        var brandColors = ImmutableDictionary.Create<string, string>()
            .Add("500", "oklch(71.2% 0.191 280)");

        // Act
        var updatedTheme = theme.AddColorPalette("brand", brandColors);

        // Assert
        // Brand color should be added
        updatedTheme.ResolveValue("--color-brand-500", []).ShouldBe("oklch(71.2% 0.191 280)");
        // Default colors should still exist
        updatedTheme.ResolveValue("--color-red-500", []).ShouldNotBeNull();
        updatedTheme.ResolveValue("--color-blue-500", []).ShouldNotBeNull();
    }
}