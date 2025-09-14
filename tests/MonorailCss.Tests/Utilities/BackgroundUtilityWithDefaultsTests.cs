using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Backgrounds;
using MonorailCss.Variants;
using Shouldly;

namespace MonorailCss.Tests.Utilities;

public class BackgroundUtilityWithDefaultsTests
{
    private readonly BackgroundColorUtility _backgroundUtility;
    private readonly MonorailCss.Theme.Theme _defaultTheme;

    public BackgroundUtilityWithDefaultsTests()
    {
        _backgroundUtility = new BackgroundColorUtility();
        _defaultTheme = new MonorailCss.Theme.Theme(); // Uses defaults
    }

    [Theory]
    [InlineData("red-50", "var(--color-red-50)")]
    [InlineData("red-500", "var(--color-red-500)")]
    [InlineData("red-950", "var(--color-red-950)")]
    [InlineData("blue-300", "var(--color-blue-300)")]
    [InlineData("blue-700", "var(--color-blue-700)")]
    [InlineData("green-500", "var(--color-green-500)")]
    [InlineData("yellow-400", "var(--color-yellow-400)")]
    [InlineData("purple-600", "var(--color-purple-600)")]
    [InlineData("gray-100", "var(--color-gray-100)")]
    [InlineData("slate-800", "var(--color-slate-800)")]
    [InlineData("zinc-300", "var(--color-zinc-300)")]
    [InlineData("neutral-500", "var(--color-neutral-500)")]
    [InlineData("stone-200", "var(--color-stone-200)")]
    public void TryCompile_WithDefaultThemeColors_GeneratesCorrectCss(string colorValue, string expectedCssValue)
    {
        // Arrange
        var candidate = new FunctionalUtility
        {
            Raw = $"bg-{colorValue}",
            Root = "bg",
            Value = CandidateValue.Named(colorValue),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        // Act
        var result = _backgroundUtility.TryCompile(candidate, _defaultTheme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("background-color");
        declaration.Value.ShouldBe(expectedCssValue);
        declaration.Important.ShouldBeFalse();
    }

    [Fact]
    public void TryCompile_WithDefaultTheme_ResolvesToActualColorValue()
    {
        // Arrange
        var candidate = new FunctionalUtility
        {
            Raw = "bg-red-500",
            Root = "bg",
            Value = CandidateValue.Named("red-500"),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        // Act
        var result = _backgroundUtility.TryCompile(candidate, _defaultTheme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();

        // The resolved CSS should be a var() reference
        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Value.ShouldBe("var(--color-red-500)");

        // And the theme should have the actual OKLCH value
        _defaultTheme.ResolveValue("--color-red-500", []).ShouldBe("oklch(63.7% 0.237 25.331)");
    }

    [Theory]
    [InlineData("black", "var(--color-black)")]
    [InlineData("white", "var(--color-white)")]
    public void TryCompile_WithSpecialDefaultColors_GeneratesCorrectCss(string colorValue, string expectedCssValue)
    {
        // Arrange
        var candidate = new FunctionalUtility
        {
            Raw = $"bg-{colorValue}",
            Root = "bg",
            Value = CandidateValue.Named(colorValue),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        // Act
        var result = _backgroundUtility.TryCompile(candidate, _defaultTheme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("background-color");
        declaration.Value.ShouldBe(expectedCssValue);
    }

    [Fact]
    public void Theme_WithCustomOverrides_UsesCustomValueInsteadOfDefault()
    {
        // Arrange - Create theme with custom red-500
        var customTheme = MonorailCss.Theme.Theme.CreateWithDefaults(
            ImmutableDictionary<string, string>.Empty
                .Add("--color-red-500", "#custom-red"));

        var candidate = new FunctionalUtility
        {
            Raw = "bg-red-500",
            Root = "bg",
            Value = CandidateValue.Named("red-500"),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        // Act
        var result = _backgroundUtility.TryCompile(candidate, customTheme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Value.ShouldBe("var(--color-red-500)");

        // The custom theme should have the override value
        customTheme.ResolveValue("--color-red-500", []).ShouldBe("#custom-red");

        // But other defaults should still be there
        customTheme.ResolveValue("--color-blue-500", []).ShouldBe("oklch(62.3% 0.214 259.815)");
    }

    [Fact]
    public void AllDefaultColorPalettes_WorkWithBackgroundUtility()
    {
        // Test that all color palettes work
        var colorPalettes = new[]
        {
            "red", "orange", "amber", "yellow", "lime", "green", "emerald",
            "teal", "cyan", "sky", "blue", "indigo", "violet", "purple",
            "fuchsia", "pink", "rose", "slate", "gray", "zinc", "neutral", "stone"
        };

        var shades = new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" };

        foreach (var palette in colorPalettes)
        {
            foreach (var shade in shades)
            {
                var candidate = new FunctionalUtility
                {
                    Raw = $"bg-{palette}-{shade}",
                    Root = "bg",
                    Value = CandidateValue.Named($"{palette}-{shade}"),
                    Variants = ImmutableArray<VariantToken>.Empty,
                    Important = false
                };

                var result = _backgroundUtility.TryCompile(candidate, _defaultTheme, out var nodes);

                result.ShouldBeTrue($"bg-{palette}-{shade} should compile");
                nodes.ShouldNotBeNull();

                var declaration = nodes[0].ShouldBeOfType<Declaration>();
                declaration.Value.ShouldBe($"var(--color-{palette}-{shade})");
            }
        }
    }
}