using MonorailCss.Candidates;
using MonorailCss.Utilities.Resolvers;
using Shouldly;

namespace MonorailCss.Tests.Utilities.Resolvers;

public class ValueResolverTests
{
    [Fact]
    public void TryResolveColor_WithSpecialValues_ReturnsCorrectCssValue()
    {
        var theme = new MonorailCss.Theme.Theme();

        ValueResolver.TryResolveColor(CandidateValue.Named("transparent"), theme, null, out var result1)
            .ShouldBeTrue();
        result1.ShouldBe("transparent");

        ValueResolver.TryResolveColor(CandidateValue.Named("current"), theme, null, out var result2)
            .ShouldBeTrue();
        result2.ShouldBe("currentColor");

        ValueResolver.TryResolveColor(CandidateValue.Named("inherit"), theme, null, out var result3)
            .ShouldBeTrue();
        result3.ShouldBe("inherit");
    }

    [Fact]
    public void TryResolveColor_WithArbitraryValue_ReturnsRawValue()
    {
        var theme = new MonorailCss.Theme.Theme();

        ValueResolver.TryResolveColor(CandidateValue.Arbitrary("#123456"), theme, null, out var result1)
            .ShouldBeTrue();
        result1.ShouldBe("#123456");

        ValueResolver.TryResolveColor(CandidateValue.Arbitrary("rgb(255, 0, 0)"), theme, null, out var result2)
            .ShouldBeTrue();
        result2.ShouldBe("rgb(255, 0, 0)");

        ValueResolver.TryResolveColor(CandidateValue.Arbitrary("hsl(180, 50%, 50%)"), theme, null, out var result3)
            .ShouldBeTrue();
        result3.ShouldBe("hsl(180, 50%, 50%)");
    }

    [Fact]
    public void TryResolveColor_WithThemeValue_ResolvesFromTheme()
    {
        var theme = new MonorailCss.Theme.Theme()
            .Add("--color-red-500", "#ef4444")
            .Add("--background-color-primary", "#1a1a1a");

        ValueResolver.TryResolveColor(
            CandidateValue.Named("red-500"),
            theme,
            ["--color"],
            out var result1)
            .ShouldBeTrue();
        result1.ShouldBe("var(--color-red-500)");

        ValueResolver.TryResolveColor(
            CandidateValue.Named("primary"),
            theme,
            ["--background-color", "--color"],
            out var result2)
            .ShouldBeTrue();
        result2.ShouldBe("var(--background-color-primary)");
    }

    [Fact]
    public void TryResolveColor_WithUnknownValue_ReturnsFalse()
    {
        var theme = new MonorailCss.Theme.Theme();

        ValueResolver.TryResolveColor(
            CandidateValue.Named("unknown-color"),
            theme,
            ["--color"],
            out var result)
            .ShouldBeFalse();
        result.ShouldBeNull();
    }

    [Fact]
    public void TryResolveSpacing_WithThemeValue_ResolvesFromTheme()
    {
        var theme = new MonorailCss.Theme.Theme()
            .Add("--spacing-4", "1rem")
            .Add("--spacing-8", "2rem");

        ValueResolver.TryResolveSpacing(
            CandidateValue.Named("4"),
            theme,
            ["--spacing"],
            out var result1)
            .ShouldBeTrue();
        result1.ShouldBe("var(--spacing-4)");

        ValueResolver.TryResolveSpacing(
            CandidateValue.Named("8"),
            theme,
            ["--spacing"],
            out var result2)
            .ShouldBeTrue();
        result2.ShouldBe("var(--spacing-8)");
    }

    [Fact]
    public void TryResolveSpacing_WithMultiplier_CalculatesValue()
    {
        var theme = new MonorailCss.Theme.Theme()
            .Add("--spacing", "0.25rem");

        ValueResolver.TryResolveSpacing(
            CandidateValue.Named("4"),
            theme,
            ["--padding"],
            out var result1)
            .ShouldBeTrue();
        result1.ShouldBe("calc(var(--spacing) * 4)");

        ValueResolver.TryResolveSpacing(
            CandidateValue.Named("0.5"),
            theme,
            ["--margin"],
            out var result2)
            .ShouldBeTrue();
        result2.ShouldBe("calc(var(--spacing) * 0.5)");

        ValueResolver.TryResolveSpacing(
            CandidateValue.Named("1.5"),
            theme,
            ["--gap"],
            out var result3)
            .ShouldBeTrue();
        result3.ShouldBe("calc(var(--spacing) * 1.5)");
    }

    [Fact]
    public void TryResolveSpacing_WithArbitraryValue_ReturnsRawValue()
    {
        var theme = new MonorailCss.Theme.Theme();

        ValueResolver.TryResolveSpacing(
            CandidateValue.Arbitrary("32px"),
            theme,
            null,
            out var result1)
            .ShouldBeTrue();
        result1.ShouldBe("32px");

        ValueResolver.TryResolveSpacing(
            CandidateValue.Arbitrary("calc(100% - 20px)"),
            theme,
            null,
            out var result2)
            .ShouldBeTrue();
        result2.ShouldBe("calc(100% - 20px)");
    }

    [Fact]
    public void TryResolveSize_WithFraction_CalculatesPercentage()
    {
        var theme = new MonorailCss.Theme.Theme();

        ValueResolver.TryResolveSize(CandidateValue.Named("1/2"), theme, null, out var result1)
            .ShouldBeTrue();
        result1.ShouldBe("50%");

        ValueResolver.TryResolveSize(CandidateValue.Named("1/3"), theme, null, out var result2)
            .ShouldBeTrue();
        result2.ShouldBe("33.33333333333333%");

        ValueResolver.TryResolveSize(CandidateValue.Named("3/4"), theme, null, out var result3)
            .ShouldBeTrue();
        result3.ShouldBe("75%");

        ValueResolver.TryResolveSize(CandidateValue.Named("2/3"), theme, null, out var result4)
            .ShouldBeTrue();
        result4.ShouldBe("66.66666666666666%");
    }

    [Fact]
    public void TryResolveSize_WithSpecialValues_ReturnsCorrectValue()
    {
        var theme = new MonorailCss.Theme.Theme();

        ValueResolver.TryResolveSize(CandidateValue.Named("full"), theme, null, out var result1)
            .ShouldBeTrue();
        result1.ShouldBe("100%");

        ValueResolver.TryResolveSize(CandidateValue.Named("screen"), theme, null, out var result2)
            .ShouldBeTrue();
        result2.ShouldBe("100vh");

        ValueResolver.TryResolveSize(CandidateValue.Named("auto"), theme, null, out var result3)
            .ShouldBeTrue();
        result3.ShouldBe("auto");

        ValueResolver.TryResolveSize(CandidateValue.Named("min"), theme, null, out var result4)
            .ShouldBeTrue();
        result4.ShouldBe("min-content");

        ValueResolver.TryResolveSize(CandidateValue.Named("max"), theme, null, out var result5)
            .ShouldBeTrue();
        result5.ShouldBe("max-content");

        ValueResolver.TryResolveSize(CandidateValue.Named("fit"), theme, null, out var result6)
            .ShouldBeTrue();
        result6.ShouldBe("fit-content");
    }

    [Fact]
    public void TryResolveSize_WithThemeValue_ResolvesFromTheme()
    {
        var theme = new MonorailCss.Theme.Theme()
            .Add("--width-sm", "24rem")
            .Add("--width-md", "28rem");

        ValueResolver.TryResolveSize(
            CandidateValue.Named("sm"),
            theme,
            ["--width"],
            out var result1)
            .ShouldBeTrue();
        result1.ShouldBe("var(--width-sm)");

        ValueResolver.TryResolveSize(
            CandidateValue.Named("md"),
            theme,
            ["--width"],
            out var result2)
            .ShouldBeTrue();
        result2.ShouldBe("var(--width-md)");
    }

    [Fact]
    public void TryResolveBorderWidth_WithArbitraryValue_ReturnsRawValue()
    {
        var theme = new MonorailCss.Theme.Theme();

        ValueResolver.TryResolveBorderWidth(
            CandidateValue.Arbitrary("3px"),
            theme,
            out var result)
            .ShouldBeTrue();
        result.ShouldBe("3px");
    }

    [Fact]
    public void TryResolveBorderRadius_WithSpecialValues_ReturnsCorrectValue()
    {
        var theme = new MonorailCss.Theme.Theme();

        ValueResolver.TryResolveBorderRadius(CandidateValue.Named("none"), theme, out var result1)
            .ShouldBeTrue();
        result1.ShouldBe("0");

        ValueResolver.TryResolveBorderRadius(CandidateValue.Named("full"), theme, out var result2)
            .ShouldBeTrue();
        result2.ShouldBe("calc(infinity * 1px)");
    }

    [Fact]
    public void TryResolveBorderRadius_WithThemeValue_ResolvesFromTheme()
    {
        var theme = new MonorailCss.Theme.Theme()
            .Add("--border-radius-md", "0.375rem")
            .Add("--border-radius-lg", "0.5rem");

        ValueResolver.TryResolveBorderRadius(
            CandidateValue.Named("md"),
            theme,
            out var result1)
            .ShouldBeTrue();
        result1.ShouldBe("var(--border-radius-md)");

        ValueResolver.TryResolveBorderRadius(
            CandidateValue.Named("lg"),
            theme,
            out var result2)
            .ShouldBeTrue();
        result2.ShouldBe("var(--border-radius-lg)");
    }

    [Fact]
    public void TryResolveBorderRadius_WithArbitraryValue_ReturnsRawValue()
    {
        var theme = new MonorailCss.Theme.Theme();

        ValueResolver.TryResolveBorderRadius(
            CandidateValue.Arbitrary("12px"),
            theme,
            out var result)
            .ShouldBeTrue();
        result.ShouldBe("12px");
    }
}