using MonorailCss.Parser;
using Shouldly;

namespace MonorailCss.Tests.Parser;

public class ArbitraryValueParserTests
{
    private readonly ArbitraryValueParser _parser = new();

    #region Parentheses Shorthand Tests

    [Theory]
    [InlineData("--my-color", "var(--my-color)")]
    [InlineData("--primary", "var(--primary)")]
    [InlineData("--spacing-4", "var(--spacing-4)")]
    public void Parse_ParenthesesShorthand_ConvertsToVarFunction(string input, string expected)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Parentheses);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe(expected);
        result.IsParenthesesShorthand.ShouldBeTrue();
    }

    [Theory]
    [InlineData("--my-color,#0088cc", "var(--my-color,#0088cc)")]
    [InlineData("--primary,red", "var(--primary,red)")]
    [InlineData("--opacity,0.5", "var(--opacity,0.5)")]
    public void Parse_ParenthesesShorthand_WithFallback_ConvertsCorrectly(string input, string expected)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Parentheses);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe(expected);
        result.IsParenthesesShorthand.ShouldBeTrue();
    }

    [Theory]
    [InlineData("--a,var(--b,#fff)", "var(--a,var(--b,#fff))")]
    [InlineData("--color,rgb(255,0,0)", "var(--color,rgb(255,0,0))")]
    public void Parse_ParenthesesShorthand_WithComplexFallback_HandlesNested(string input, string expected)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Parentheses);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("my-color")]
    [InlineData("primary")]
    [InlineData("red")]
    public void Parse_ParenthesesShorthand_WithoutDashDash_RejectsInvalid(string input)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Parentheses);

        result.IsValid.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNull();
        result.ErrorMessage!.ShouldContain("CSS variables");
        result.ErrorMessage.ShouldContain("--prefix");
    }

    #endregion

    #region Data Type Hints Tests

    [Theory]
    [InlineData("color:var(--my-color)", "var(--my-color)", "color")]
    [InlineData("length:100px", "100px", "length")]
    [InlineData("angle:45deg", "45deg", "angle")]
    public void Parse_BracketValue_WithDataTypeHint_ExtractsTypeAndValue(string input, string expectedValue, string expectedType)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Brackets);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe(expectedValue);
        result.DataTypeHint.ShouldBe(expectedType);
    }

    [Theory]
    [InlineData("color:--my-color", "var(--my-color)", "color")]
    [InlineData("length:--spacing-4", "var(--spacing-4)", "length")]
    public void Parse_ParenthesesShorthand_WithDataTypeHint_ConvertsCorrectly(string input, string expectedValue, string expectedType)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Parentheses);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe(expectedValue);
        result.DataTypeHint.ShouldBe(expectedType);
        result.IsParenthesesShorthand.ShouldBeTrue();
    }

    [Fact]
    public void Parse_ParenthesesShorthand_WithDataTypeHint_WithoutDashDash_RejectsInvalid()
    {
        var result = _parser.Parse("color:red", ArbitraryValueType.Parentheses);

        result.IsValid.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNull();
        result.ErrorMessage!.ShouldContain("CSS variable");
    }

    #endregion

    #region Underscore to Space Conversion Tests

    [Theory]
    [InlineData("hello_world", "hello world")]
    [InlineData("foo_bar_baz", "foo bar baz")]
    [InlineData("margin_10px_20px", "margin 10px 20px")]
    public void Parse_BracketValue_ConvertsUnderscoresToSpaces(string input, string expected)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Brackets);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("hello\\_world", "hello_world")]
    [InlineData("escaped\\_underscore\\_test", "escaped_underscore_test")]
    public void Parse_BracketValue_PreservesEscapedUnderscores(string input, string expected)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Brackets);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("url(https://example.com/some_page)", "url(https://example.com/some_page)")]
    [InlineData("url(/path/to/file_name.jpg)", "url(/path/to/file_name.jpg)")]
    public void Parse_BracketValue_PreservesUnderscoresInUrls(string input, string expected)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Brackets);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("--my-color,fallback_value", "var(--my-color,fallback value)")]
    [InlineData("--spacing,10px_20px", "var(--spacing,10px 20px)")]
    public void Parse_ParenthesesShorthand_ConvertsUnderscoresInFallback(string input, string expected)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Parentheses);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    #endregion

    #region Complex Expression Tests

    [Theory]
    [InlineData("calc(100%-2rem)", true)]
    [InlineData("calc(var(--foo)*0.2)", true)]
    [InlineData("calc((100vh-64px)/2)", true)]
    public void TryParseCalcExpression_ValidExpressions_ReturnsTrue(string input, bool expectedValid)
    {
        var result = _parser.TryParseCalcExpression(input, out var processed);

        result.ShouldBe(expectedValid);
        if (expectedValid)
        {
            processed.ShouldNotBeNull();
        }
    }

    [Theory]
    [InlineData("calc(100%_-_2rem)", "calc(100% - 2rem)")]
    [InlineData("calc(50%_+_10px)", "calc(50% + 10px)")]
    public void TryParseCalcExpression_ConvertsUnderscores(string input, string expected)
    {
        var result = _parser.TryParseCalcExpression(input, out var processed);

        result.ShouldBeTrue();
        processed.ShouldBe(expected);
    }

    [Theory]
    [InlineData("theme(--spacing-1_5)", true)]
    [InlineData("theme(colors.red.500)", true)]
    public void TryParseThemeFunction_ValidExpressions_ReturnsTrue(string input, bool expectedValid)
    {
        var result = _parser.TryParseThemeFunction(input, out var processed);

        result.ShouldBe(expectedValid);
        if (expectedValid)
        {
            processed.ShouldNotBeNull();
        }
    }

    [Fact]
    public void TryParseThemeFunction_PreservesUnderscoresInPath()
    {
        var result = _parser.TryParseThemeFunction("theme(--spacing-1_5)", out var processed);

        result.ShouldBeTrue();
        processed.ShouldBe("theme(--spacing-1_5)");
    }

    [Fact]
    public void TryParseThemeFunction_ConvertsUnderscoresInFallback()
    {
        var result = _parser.TryParseThemeFunction("theme(--spacing,default_value)", out var processed);

        result.ShouldBeTrue();
        processed.ShouldBe("theme(--spacing,default value)");
    }

    [Theory]
    [InlineData("var(--navbar-height)", "var(--navbar-height)")]
    [InlineData("var(--sidebar-width)", "var(--sidebar-width)")]
    [InlineData("var(--my-color,#ff0000)", "var(--my-color,#ff0000)")]
    [InlineData("var(--spacing,1rem)", "var(--spacing,1rem)")]
    public void Parse_BracketValue_WithVarFunction_ParsesCorrectly(string input, string expected)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Brackets);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("calc(100vh-var(--navbar-height))", "calc(100vh - var(--navbar-height))")]
    [InlineData("min(100%,var(--max-width))", "min(100%, var(--max-width))")]
    [InlineData("max(50vw,var(--min-width))", "max(50vw, var(--min-width))")]
    [InlineData("clamp(10rem,var(--size),20rem)", "clamp(10rem, var(--size), 20rem)")]
    public void Parse_BracketValue_WithNestedVarInMathFunctions_ParsesCorrectly(string input, string expected)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Brackets);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    #endregion

    #region Invalid Cases Tests

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("_")]
    public void Parse_EmptyOrInvalidValue_RejectsWithError(string input)
    {
        var bracketResult = _parser.Parse(input, ArbitraryValueType.Brackets);
        bracketResult.IsValid.ShouldBeFalse();

        if (!string.IsNullOrEmpty(input))
        {
            var parenResult = _parser.Parse(input, ArbitraryValueType.Parentheses);
            parenResult.IsValid.ShouldBeFalse();
        }
    }

    [Theory]
    [InlineData("red;color:blue")]
    [InlineData("color:red}html{color:blue")]
    [InlineData("value;injection")]
    public void Parse_BracketValue_WithInvalidCharacters_RejectsWithError(string input)
    {
        var result = _parser.Parse(input, ArbitraryValueType.Brackets);

        result.IsValid.ShouldBeFalse();
        result.ErrorMessage.ShouldNotBeNull();
        result.ErrorMessage!.ShouldContain("invalid characters");
    }

    [Theory]
    [InlineData("calc(()")]
    [InlineData("calc((100%)")]
    [InlineData("theme((colors")]
    public void TryParseCalcExpression_UnbalancedParentheses_ReturnsFalse(string input)
    {
        var result = _parser.TryParseCalcExpression(input, out var processed);

        result.ShouldBeFalse();
        processed.ShouldBeNull();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Parse_ComplexNestedExpression_HandlesCorrectly()
    {
        var input = "calc(var(--spacing,theme(spacing.4))*2)";
        var result = _parser.Parse(input, ArbitraryValueType.Brackets);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }

    [Fact]
    public void Parse_StringWithQuotes_PreservesContent()
    {
        var input = "\"hello_world\"";
        var result = _parser.Parse(input, ArbitraryValueType.Brackets);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe("\"hello world\"");
    }

    [Fact]
    public void Parse_ComplexCssVariableFallback_HandlesMultipleLevels()
    {
        var input = "--a,var(--b,var(--c,red))";
        var result = _parser.Parse(input, ArbitraryValueType.Parentheses);

        result.IsValid.ShouldBeTrue();
        result.Value.ShouldBe("var(--a,var(--b,var(--c,red)))");
    }

    #endregion
}