using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities;
using MonorailCss.Utilities.Typography;
using MonorailCss.Variants;
using Shouldly;

namespace MonorailCss.Tests.Utilities;

public class TextColorUtilityTests
{
    private readonly TextUtility _textUtility;
    private readonly MonorailCss.Theme.Theme _theme;

    public TextColorUtilityTests()
    {
        _textUtility = new TextUtility();
        // Use Theme.CreateEmpty() for explicit test values
        _theme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-red-500", "#ef4444")
            .Add("--color-blue-700", "#1d4ed8")
            .Add("--color-green-300", "#86efac")
            .Add("--text-color-primary", "#ffffff")
            .Add("--text-color-secondary", "#e5e5e5");
    }

    [Theory]
    [InlineData("red-500", "var(--color-red-500)")]
    [InlineData("blue-700", "var(--color-blue-700)")]
    [InlineData("green-300", "var(--color-green-300)")]
    public void TryCompile_WithThemeColors_GeneratesCorrectCss(string colorValue, string expectedCssValue)
    {
        var candidate = new FunctionalUtility
        {
            Raw = $"text-{colorValue}",
            Root = "text",
            Value = CandidateValue.Named(colorValue),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _textUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("color");
        declaration.Value.ShouldBe(expectedCssValue);
        declaration.Important.ShouldBeFalse();
    }

    [Theory]
    [InlineData("primary", "var(--text-color-primary)")]
    [InlineData("secondary", "var(--text-color-secondary)")]
    public void TryCompile_WithTextColorNamespace_PrefersTextColorOverColor(string colorValue, string expectedCssValue)
    {
        var candidate = new FunctionalUtility
        {
            Raw = $"text-{colorValue}",
            Root = "text",
            Value = CandidateValue.Named(colorValue),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _textUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("color");
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
            Raw = $"text-{specialValue}",
            Root = "text",
            Value = CandidateValue.Named(specialValue),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _textUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("color");
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
            Raw = $"text-[{arbitraryValue}]",
            Root = "text",
            Value = CandidateValue.Arbitrary(arbitraryValue),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _textUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("color");
        declaration.Value.ShouldBe(expectedCssValue);
    }

    [Fact]
    public void TryCompile_WithImportantFlag_GeneratesImportantDeclaration()
    {
        var candidate = new FunctionalUtility
        {
            Raw = "!text-red-500",
            Root = "text",
            Value = CandidateValue.Named("red-500"),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = true
        };

        var result = _textUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("color");
        declaration.Value.ShouldBe("var(--color-red-500)");
        declaration.Important.ShouldBeTrue();
    }

    [Fact]
    public void TryCompile_WithUnknownColor_ReturnsFalse()
    {
        var candidate = new FunctionalUtility
        {
            Raw = "text-unknown-color",
            Root = "text",
            Value = CandidateValue.Named("unknown-color"),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _textUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeFalse();
        nodes.ShouldBeNull();
    }

    [Fact]
    public void TryCompile_WithNonTextUtility_ReturnsFalse()
    {
        var candidate = new FunctionalUtility
        {
            Raw = "bg-red-500",
            Root = "bg",
            Value = CandidateValue.Named("red-500"),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        var result = _textUtility.TryCompile(candidate, _theme, out var nodes);

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

        var result = _textUtility.TryCompile(candidate, _theme, out var nodes);

        result.ShouldBeFalse();
        nodes.ShouldBeNull();
    }




    [Fact]
    public void Priority_ReturnsNamespaceHandler()
    {
        var priority = _textUtility.Priority;

        priority.ShouldBe(UtilityPriority.NamespaceHandler);
    }

    [Fact]
    public void DoesNotConflictWithTextSizeUtilities()
    {
        // Ensure text-lg, text-sm, etc. are processed as font-size, not as color
        var sizeUtilities = new[] { "lg", "sm", "xs", "xl", "2xl", "base" };

        foreach (var size in sizeUtilities)
        {
            var candidate = new FunctionalUtility
            {
                Raw = $"text-{size}",
                Root = "text",
                Value = CandidateValue.Named(size),
                Variants = ImmutableArray<VariantToken>.Empty,
                Important = false
            };

            var result = _textUtility.TryCompile(candidate, _theme, out var nodes);

            // TextUtility should process these as font-size utilities
            result.ShouldBeTrue($"text-{size} should be processed as font-size");
            nodes.ShouldNotBeNull();
            nodes.Count.ShouldBeGreaterThan(0);

            // Should generate font-size, not color
            var hasColor = nodes.Any(n => n is Declaration d && d.Property == "color");
            var hasFontSize = nodes.Any(n => n is Declaration d && d.Property == "font-size");

            hasColor.ShouldBeFalse($"text-{size} should not generate color property");
            hasFontSize.ShouldBeTrue($"text-{size} should generate font-size property");
        }
    }
}