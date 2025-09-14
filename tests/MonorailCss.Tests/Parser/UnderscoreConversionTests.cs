using MonorailCss.Parser;
using Shouldly;

namespace MonorailCss.Tests.Parser;

public class UnderscoreConversionTests
{
    private readonly ArbitraryValueParser _parser = new();

    [Theory]
    [InlineData("foo_bar", "foo bar")]
    [InlineData("__foo__bar__", "  foo  bar  ")]
    [InlineData("hello_world", "hello world")]
    [InlineData("margin_10px_20px", "margin 10px 20px")]
    public void DecodeArbitraryValue_ReplacesUnderscoresWithSpaces(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("foo\\_bar", "foo_bar")]
    [InlineData("escaped\\_underscore\\_test", "escaped_underscore_test")]
    [InlineData("hello\\\\_world", "hello\\_world")] // Double escaped
    public void DecodeArbitraryValue_PreservesEscapedUnderscores(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("url(./my_file.jpg)", "url(./my_file.jpg)")]
    [InlineData("no-repeat_url(./my_file.jpg)", "no-repeat url(./my_file.jpg)")]
    [InlineData("url(https://example.com/some_page)", "url(https://example.com/some_page)")]
    [InlineData("background-image:url(./path_to_file.png)", "background-image:url(./path_to_file.png)")]
    public void DecodeArbitraryValue_PreservesUnderscoresInUrlFunctions(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("var(--spacing-1_5)", "var(--spacing-1_5)")]
    [InlineData("var(--spacing-1_5,_1rem)", "var(--spacing-1_5, 1rem)")]
    [InlineData("var(--spacing-1_5,_var(--spacing-2_5,_1rem))", "var(--spacing-1_5, var(--spacing-2_5, 1rem))")]
    [InlineData("var(--my_custom_property)", "var(--my_custom_property)")]
    [InlineData("var(--foo,_fallback_value)", "var(--foo, fallback value)")]
    public void DecodeArbitraryValue_PreservesUnderscoresInVarFirstArgument(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("theme(--spacing-1_5)", "theme(--spacing-1_5)")]
    [InlineData("theme(--spacing-1_5,_1rem)", "theme(--spacing-1_5, 1rem)")]
    [InlineData("theme(--spacing-1_5,_theme(--spacing-2_5,_1rem))", "theme(--spacing-1_5, theme(--spacing-2_5, 1rem))")]
    [InlineData("theme(spacing.foo_bar)", "theme(spacing.foo_bar)")]
    public void DecodeArbitraryValue_PreservesUnderscoresInThemeFirstArgument(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("\"hello_world\"", "\"hello world\"")]
    [InlineData("'hello_world'", "'hello world'")]
    [InlineData("content:\"hello_world\"", "content:\"hello world\"")]
    [InlineData("before:content:\"foo_bar\"", "before:content:\"foo bar\"")]
    public void DecodeArbitraryValue_ConvertsUnderscoresInStrings(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("linear-gradient(to_right,_#000,_#fff)", "linear-gradient(to right, #000, #fff)")]
    [InlineData("rgb(255_255_255)", "rgb(255 255 255)")]
    [InlineData("rgba(255_255_255_/_0.5)", "rgba(255 255 255 / 0.5)")]
    [InlineData("hsl(120deg_100%_50%)", "hsl(120deg 100% 50%)")]
    public void DecodeArbitraryValue_ConvertsUnderscoresInCssFunctions(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("calc(100%_-_20px)", "calc(100% - 20px)")]
    [InlineData("min(100px,_50%)", "min(100px, 50%)")]
    [InlineData("max(10rem,_20vh)", "max(10rem, 20vh)")]
    [InlineData("clamp(1rem,_2vw,_3rem)", "clamp(1rem, 2vw, 3rem)")]
    public void DecodeArbitraryValue_ConvertsUnderscoresAndPreservesFunctionality(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("[content-start]_calc(100%-1px)_[content-end]_minmax(1rem,1fr)",
                "[content-start] calc(100% - 1px) [content-end] minmax(1rem,1fr)")]
    [InlineData("repeat(auto-fill,_minmax(250px,_1fr))",
                "repeat(auto-fill, minmax(250px, 1fr))")]
    public void DecodeArbitraryValue_HandlesComplexGridValues(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("var(--foo)", "var(--foo)")]
    [InlineData("var(--headings-h1-size)", "var(--headings-h1-size)")]
    [InlineData("var(--color-primary)", "var(--color-primary)")]
    public void DecodeArbitraryValue_LeavesVarFunctionsAsIs(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }
}