using MonorailCss.Css;
using Shouldly;

namespace MonorailCss.Tests.Css;

public class TailwindFunctionExpanderTests
{
    [Theory]
    [InlineData(
        "--alpha(var(--lumex-divider) / var(--lumex-opacity-divider))",
        "color-mix(in oklab, var(--lumex-divider) var(--lumex-opacity-divider), transparent)")]
    [InlineData(
        "--alpha(#fff / 50%)",
        "color-mix(in oklab, #fff 50%, transparent)")]
    [InlineData(
        "--alpha(var(--c) / 0.5)",
        "color-mix(in oklab, var(--c) 0.5, transparent)")]
    [InlineData(
        "--alpha(oklch(60% 0.2 250) / 30%)",
        "color-mix(in oklab, oklch(60% 0.2 250) 30%, transparent)")]
    public void Expand_Alpha(string input, string expected)
    {
        TailwindFunctionExpander.Expand(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("--spacing(5)", "calc(var(--spacing) * 5)")]
    [InlineData("--spacing(2.5)", "calc(var(--spacing) * 2.5)")]
    [InlineData("--spacing(var(--n))", "calc(var(--spacing) * var(--n))")]
    public void Expand_Spacing(string input, string expected)
    {
        TailwindFunctionExpander.Expand(input).ShouldBe(expected);
    }

    [Fact]
    public void Expand_Spacing_Inside_Declaration_Composite_Value()
    {
        TailwindFunctionExpander.Expand("--spacing(2) --spacing(4)")
            .ShouldBe("calc(var(--spacing) * 2) calc(var(--spacing) * 4)");
    }

    [Fact]
    public void Expand_Nested_Spacing_Inside_Alpha()
    {
        // The inner --spacing(2) must resolve before --alpha sees it, otherwise the split-on-'/'
        // would split the wrong way.
        TailwindFunctionExpander.Expand("--alpha(var(--c) / --spacing(2))")
            .ShouldBe("color-mix(in oklab, var(--c) calc(var(--spacing) * 2), transparent)");
    }

    [Fact]
    public void Expand_PassThrough_When_No_Functions_Present()
    {
        var input = "var(--lumex-primary)";
        TailwindFunctionExpander.Expand(input).ShouldBeSameAs(input);
    }

    [Fact]
    public void Expand_PassThrough_For_PlainValue()
    {
        var input = "1px solid red";
        TailwindFunctionExpander.Expand(input).ShouldBeSameAs(input);
    }

    [Fact]
    public void Expand_Is_Idempotent()
    {
        const string input = "--alpha(var(--c) / 50%)";
        var once = TailwindFunctionExpander.Expand(input);
        var twice = TailwindFunctionExpander.Expand(once);
        twice.ShouldBe(once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Expand_Handles_NullOrEmpty(string? input)
    {
        TailwindFunctionExpander.Expand(input!).ShouldBe(input!);
    }

    [Fact]
    public void Expand_Alpha_Without_Slash_Leaves_Value_Alone()
    {
        // Malformed --alpha(...) shouldn't silently rewrite to something nonsensical.
        const string input = "--alpha(var(--c))";
        TailwindFunctionExpander.Expand(input).ShouldBe(input);
    }

    [Fact]
    public void Expand_Alpha_Surrounded_By_Other_Tokens()
    {
        TailwindFunctionExpander.Expand("inset 0 1px 0 0 --alpha(var(--c) / 50%)")
            .ShouldBe("inset 0 1px 0 0 color-mix(in oklab, var(--c) 50%, transparent)");
    }
}
