using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities;
using MonorailCss.Utilities.Backgrounds;
using MonorailCss.Variants;
using Shouldly;

namespace MonorailCss.Tests.Utilities;

public class BackgroundColorUtilityTests
{
    private readonly BackgroundColorUtility _backgroundUtility;
    private readonly MonorailCss.Theme.Theme _theme;

    public BackgroundColorUtilityTests()
    {
        _backgroundUtility = new BackgroundColorUtility();
        // Use Theme.CreateEmpty() for explicit test values
        _theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444")
            .Add("--color-blue-700", "#1d4ed8")
            .Add("--color-green-300", "#86efac")
            .Add("--background-color-primary", "#1a1a1a")
            .Add("--background-color-secondary", "#2d2d2d");
    }

    [Theory]
    [InlineData("red-500", "var(--color-red-500)")]
    [InlineData("blue-700", "var(--color-blue-700)")]
    [InlineData("green-300", "var(--color-green-300)")]
    public void TryCompile_WithThemeColors_GeneratesCorrectCss(string colorValue, string expectedCssValue)
    {
        var candidate = new FunctionalUtility
        {
            Raw = $"bg-{colorValue}",
            Root = "bg",
            Value = CandidateValue.Named(colorValue),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _backgroundUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("background-color");
        declaration.Value.ShouldBe(expectedCssValue);
        declaration.Important.ShouldBeFalse();
    }

    [Theory]
    [InlineData("primary", "var(--background-color-primary)")]
    [InlineData("secondary", "var(--background-color-secondary)")]
    public void TryCompile_WithBackgroundColorNamespace_PrefersBackgroundColorOverColor(string colorValue, string expectedCssValue)
    {
        var candidate = new FunctionalUtility
        {
            Raw = $"bg-{colorValue}",
            Root = "bg",
            Value = CandidateValue.Named(colorValue),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _backgroundUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("background-color");
        declaration.Value.ShouldBe(expectedCssValue);
    }

    [Theory]
    [InlineData("transparent", "transparent")]
    [InlineData("current", "currentColor")]
    [InlineData("inherit", "inherit")]
    public void TryCompile_WithSpecialValues_GeneratesCorrectCss(string specialValue, string expectedCssValue)
    {
        var candidate = new FunctionalUtility
        {
            Raw = $"bg-{specialValue}",
            Root = "bg",
            Value = CandidateValue.Named(specialValue),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _backgroundUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("background-color");
        declaration.Value.ShouldBe(expectedCssValue);
    }

    [Theory]
    [InlineData("#123456", "#123456")]
    [InlineData("rgb(255,0,0)", "rgb(255,0,0)")]
    [InlineData("hsl(180,50%,50%)", "hsl(180,50%,50%)")]
    [InlineData("rgba(255,0,0,0.5)", "rgba(255,0,0,0.5)")]
    public void TryCompile_WithArbitraryValues_GeneratesCorrectCss(string arbitraryValue, string expectedCssValue)
    {
        var candidate = new FunctionalUtility
        {
            Raw = $"bg-[{arbitraryValue}]",
            Root = "bg",
            Value = CandidateValue.Arbitrary(arbitraryValue),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _backgroundUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("background-color");
        declaration.Value.ShouldBe(expectedCssValue);
    }

    [Fact]
    public void TryCompile_WithImportantFlag_GeneratesImportantDeclaration()
    {
        var candidate = new FunctionalUtility
        {
            Raw = "!bg-red-500",
            Root = "bg",
            Value = CandidateValue.Named("red-500"),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = true
        };

        var result = _backgroundUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("background-color");
        declaration.Value.ShouldBe("var(--color-red-500)");
        declaration.Important.ShouldBeTrue();
    }

    [Fact]
    public void TryCompile_WithUnknownColor_ReturnsFalse()
    {
        var candidate = new FunctionalUtility
        {
            Raw = "bg-unknown-color",
            Root = "bg",
            Value = CandidateValue.Named("unknown-color"),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _backgroundUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeFalse();
        nodes.ShouldBeNull();
    }

    [Fact]
    public void TryCompile_WithNonBgUtility_ReturnsFalse()
    {
        var candidate = new FunctionalUtility
        {
            Raw = "text-red-500",
            Root = "text",
            Value = CandidateValue.Named("red-500"),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _backgroundUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeFalse();
        nodes.ShouldBeNull();
    }

    [Fact]
    public void TryCompile_WithStaticUtility_ReturnsFalse()
    {
        var candidate = new StaticUtility
        {
            Raw = "block",
            Root = "block",
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _backgroundUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeFalse();
        nodes.ShouldBeNull();
    }




    [Fact]
    public void Priority_ReturnsNamespaceHandler()
    {
        var priority = _backgroundUtility.Priority;

        priority.ShouldBe(UtilityPriority.NamespaceHandler);
    }
}