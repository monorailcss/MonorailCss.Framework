using MonorailCss.Parser;
using Shouldly;

namespace MonorailCss.Tests.Parser;

public class MathOperatorTests
{
    private readonly ArbitraryValueParser _parser = new();

    [Theory]
    [InlineData("calc(1+2)", "calc(1 + 2)")]
    [InlineData("calc(100%+1rem)", "calc(100% + 1rem)")]
    [InlineData("calc(1+calc(100%-20px))", "calc(1 + calc(100% - 20px))")]
    [InlineData("calc(var(--headings-h1-size)*100)", "calc(var(--headings-h1-size) * 100)")]
    [InlineData("calc(var(--headings-h1-size)*calc(100%+50%))", "calc(var(--headings-h1-size) * calc(100% + 50%))")]
    public void DecodeArbitraryValue_AddsSpacesAroundMathOperatorsInCalc(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("min(1+2)", "min(1 + 2)")]
    [InlineData("max(1+2)", "max(1 + 2)")]
    [InlineData("clamp(1+2,1+3,1+4)", "clamp(1 + 2, 1 + 3, 1 + 4)")]
    public void DecodeArbitraryValue_AddsSpacesAroundMathOperatorsInMathFunctions(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("calc(1px*(7--12/24))", "calc(1px * (7 - -12 / 24))")]
    [InlineData("calc((7-32)/(1400-782))", "calc((7 - 32) / (1400 - 782))")]
    [InlineData("calc((704-320)/(1400-782))", "calc((704 - 320) / (1400 - 782))")]
    [InlineData("calc(24px+-1rem)", "calc(24px + -1rem)")]
    [InlineData("calc(24px+(-1rem))", "calc(24px + (-1rem))")]
    public void DecodeArbitraryValue_HandlesComplexMathExpressions(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("calc(24px_+_-1rem)", "calc(24px + -1rem)")]
    [InlineData("calc(24px_+_(-1rem))", "calc(24px + (-1rem))")]
    [InlineData("calc(100%_-_20px)", "calc(100% - 20px)")]
    public void DecodeArbitraryValue_ConvertsUnderscoresAndAddsSpacesAroundOperators(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("calc(100%-var(--foo))", "calc(100% - var(--foo))")]
    [InlineData("calc(100PX-theme(spacing.1))", "calc(100PX - theme(spacing.1))")]
    [InlineData("calc(1rem-theme(spacing.1-bar))", "calc(1rem - theme(spacing.1-bar))")]
    public void DecodeArbitraryValue_HandlesVariablesAndThemeFunctionsInCalc(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("min(fit-content,calc(100dvh-4rem))", "min(fit-content, calc(100dvh - 4rem))")]
    [InlineData("min(theme(spacing.foo-bar),fit-content,calc(20*calc(40-30)))",
                "min(theme(spacing.foo-bar), fit-content, calc(20 * calc(40 - 30)))")]
    [InlineData("min(fit-content,calc(100dvh-4rem)-calc(50dvh--2px))",
                "min(fit-content, calc(100dvh - 4rem) - calc(50dvh - -2px))")]
    public void DecodeArbitraryValue_PreservesCssKeywords(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("clamp(-3px+4px,-3px+4px,-3px+4px)", "clamp(-3px + 4px, -3px + 4px, -3px + 4px)")]
    [InlineData("clamp(-10e3-var(--foo),calc-size(max-content),var(--foo)+-10e3)",
                "clamp(-10e3 - var(--foo), calc-size(max-content), var(--foo) + -10e3)")]
    public void DecodeArbitraryValue_HandlesNegativeNumbersAfterCommas(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("calc(var(--foo-bar-bar)*2)", "calc(var(--foo-bar-bar) * 2)")]
    [InlineData("calc(env(safe-area-inset-bottom)*2)", "calc(env(safe-area-inset-bottom) * 2)")]
    public void DecodeArbitraryValue_PreventFormattingInsideVarAndEnv(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("minmax(min-content,25%)", "minmax(min-content,25%)")]
    [InlineData("fit-content(min(max-content,max(min-content,calc(20px+1em))))",
                "fit-content(min(max-content, max(min-content, calc(20px + 1em))))")]
    public void DecodeArbitraryValue_HandlesNonMathFunctions(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("env(safe-area-inset-bottom,calc(10px+20px))", "env(safe-area-inset-bottom,calc(10px + 20px))")]
    [InlineData("calc(env(safe-area-inset-bottom,calc(10px+20px))+5px)",
                "calc(env(safe-area-inset-bottom,calc(10px + 20px)) + 5px)")]
    public void DecodeArbitraryValue_FormatCalcNestedInEnv(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("round(1+2,1+3)", "round(1 + 2, 1 + 3)")]
    [InlineData("round(to-zero,1+2,1+3)", "round(to-zero, 1 + 2, 1 + 3)")]
    public void DecodeArbitraryValue_HandlesRoundFunction(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("radial-gradient(calc(1+2)),radial-gradient(calc(1+2))",
                "radial-gradient(calc(1 + 2)),radial-gradient(calc(1 + 2))")]
    [InlineData("w-[calc(anchor-size(width)+8px)]", "w-[calc(anchor-size(width) + 8px)]")]
    [InlineData("w-[calc(anchor-size(foo(bar))+8px)]", "w-[calc(anchor-size(foo(bar)) + 8px)]")]
    public void DecodeArbitraryValue_HandlesComplexNestedFunctions(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("calc(var(--10-10px,calc(-20px-(-30px--40px)-50px)))",
                "calc(var(--10-10px,calc(-20px - (-30px - -40px) - 50px)))")]
    [InlineData("calc(theme(spacing.1-bar))", "calc(theme(spacing.1-bar))")]
    [InlineData("calc(theme(spacing.foo-2))", "calc(theme(spacing.foo-2))")]
    [InlineData("calc(theme(spacing.foo-bar))", "calc(theme(spacing.foo-bar))")]
    public void DecodeArbitraryValue_PreservesThemeAndVarArguments(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("atan(1 + -infinity)", "atan(1 + -infinity)")]
    [InlineData("env((safe-area-inset-bottom))", "env((safe-area-inset-bottom))")]
    public void DecodeArbitraryValue_HandlesSpecialCases(string input, string expected)
    {
        var result = _parser.DecodeArbitraryValue(input);
        result.ShouldBe(expected);
    }
}