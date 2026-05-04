using MonorailCss.Parser;
using Shouldly;

namespace MonorailCss.Tests.Parser;

// Direct port of Tailwind v4's decode-arbitrary-value.test.ts (165 lines, ~40 cases).
// Source: b:\tailwindcss\packages\tailwindcss\src\utils\decode-arbitrary-value.test.ts
// Mirror exists so we catch divergences with Tailwind's decoder behavior verbatim.
public class DecodeArbitraryValueTests
{
    private readonly ArbitraryValueParser _parser = new();

    #region Underscore handling

    [Theory]
    [InlineData("foo_bar", "foo bar")]
    [InlineData("__foo__bar__", "  foo  bar  ")]
    public void DecodeArbitraryValue_ReplacesUnderscoresWithSpaces(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    [InlineData("foo\\_bar", "foo_bar")]
    public void DecodeArbitraryValue_PreservesEscapedUnderscores(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    [InlineData("url(./my_file.jpg)", "url(./my_file.jpg)")]
    [InlineData("no-repeat_url(./my_file.jpg)", "no-repeat url(./my_file.jpg)")]
    public void DecodeArbitraryValue_DoesNotReplaceUnderscoresInsideUrl(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    [InlineData("var(--spacing-1_5)", "var(--spacing-1_5)")]
    [InlineData("var(--spacing-1_5,_1rem)", "var(--spacing-1_5, 1rem)")]
    [InlineData("var(--spacing-1_5,_var(--spacing-2_5,_1rem))", "var(--spacing-1_5, var(--spacing-2_5, 1rem))")]
    public void DecodeArbitraryValue_PreservesUnderscoresInVarFirstArg(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    [InlineData("theme(--spacing-1_5)", "theme(--spacing-1_5)")]
    [InlineData("theme(--spacing-1_5,_1rem)", "theme(--spacing-1_5, 1rem)")]
    [InlineData("theme(--spacing-1_5,_theme(--spacing-2_5,_1rem))", "theme(--spacing-1_5, theme(--spacing-2_5, 1rem))")]
    public void DecodeArbitraryValue_PreservesUnderscoresInThemeFirstArg(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    [InlineData("var(--foo)", "var(--foo)")]
    [InlineData("var(--headings-h1-size)", "var(--headings-h1-size)")]
    public void DecodeArbitraryValue_LeavesVarFunctionsAsIs(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    #endregion

    #region Math operator spacing

    [Theory]
    // calc(...) gets spaces around operators
    [InlineData("calc(1+2)", "calc(1 + 2)")]
    [InlineData("calc(100%+1rem)", "calc(100% + 1rem)")]
    [InlineData("calc(1+calc(100%-20px))", "calc(1 + calc(100% - 20px))")]
    [InlineData("calc(var(--headings-h1-size)*100)", "calc(var(--headings-h1-size) * 100)")]
    [InlineData("calc(var(--headings-h1-size)*calc(100%+50%))", "calc(var(--headings-h1-size) * calc(100% + 50%))")]
    [InlineData("min(1+2)", "min(1 + 2)")]
    [InlineData("max(1+2)", "max(1 + 2)")]
    [InlineData("clamp(1+2,1+3,1+4)", "clamp(1 + 2, 1 + 3, 1 + 4)")]
    [InlineData("var(--width, calc(100%+1rem))", "var(--width, calc(100% + 1rem))")]
    [InlineData("calc(1px*(7--12/24))", "calc(1px * (7 - -12 / 24))")]
    [InlineData("calc((7-32)/(1400-782))", "calc((7 - 32) / (1400 - 782))")]
    [InlineData("calc((7-3)/(1400-782))", "calc((7 - 3) / (1400 - 782))")]
    [InlineData("calc((70-3)/(1400-782))", "calc((70 - 3) / (1400 - 782))")]
    [InlineData("calc((70-32)/(1400-782))", "calc((70 - 32) / (1400 - 782))")]
    [InlineData("calc((704-3)/(1400-782))", "calc((704 - 3) / (1400 - 782))")]
    [InlineData("calc((704-320))", "calc((704 - 320))")]
    [InlineData("calc((704-320)/1)", "calc((704 - 320) / 1)")]
    [InlineData("calc((704-320)/(1400-782))", "calc((704 - 320) / (1400 - 782))")]
    [InlineData("calc(24px+-1rem)", "calc(24px + -1rem)")]
    [InlineData("calc(24px+(-1rem))", "calc(24px + (-1rem))")]
    [InlineData("calc(24px_+_-1rem)", "calc(24px + -1rem)")]
    [InlineData("calc(24px_+_(-1rem))", "calc(24px + (-1rem))")]
    [InlineData("calc(var(--10-10px,calc(-20px-(-30px--40px)-50px)))", "calc(var(--10-10px,calc(-20px - (-30px - -40px) - 50px)))")]
    public void DecodeArbitraryValue_AddsSpacesAroundCalcOperators(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    // theme(spacing.foo) is preserved as-is (no operator spacing inside theme paths)
    [InlineData("calc(theme(spacing.1-bar))", "calc(theme(spacing.1-bar))")]
    [InlineData("theme(spacing.1-bar)", "theme(spacing.1-bar)")]
    [InlineData("calc(1rem-theme(spacing.1-bar))", "calc(1rem - theme(spacing.1-bar))")]
    [InlineData("calc(theme(spacing.foo-2))", "calc(theme(spacing.foo-2))")]
    [InlineData("calc(theme(spacing.foo-bar))", "calc(theme(spacing.foo-bar))")]
    public void DecodeArbitraryValue_PreservesThemePathsInsideCalc(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    [InlineData("calc(100%-var(--foo))", "calc(100% - var(--foo))")]
    public void DecodeArbitraryValue_HandlesPercentages(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    [InlineData("calc(100PX-theme(spacing.1))", "calc(100PX - theme(spacing.1))")]
    public void DecodeArbitraryValue_HandlesUppercaseUnits(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    // Preserve CSS keyword tokens (fit-content, max-content, min-content) without splitting around hyphens
    [InlineData("min(fit-content,calc(100dvh-4rem))", "min(fit-content, calc(100dvh - 4rem))")]
    [InlineData("min(theme(spacing.foo-bar),fit-content,calc(20*calc(40-30)))", "min(theme(spacing.foo-bar), fit-content, calc(20 * calc(40 - 30)))")]
    [InlineData("min(fit-content,calc(100dvh-4rem)-calc(50dvh--2px))", "min(fit-content, calc(100dvh - 4rem) - calc(50dvh - -2px))")]
    [InlineData("min(-3.4e-2-var(--foo),calc-size(auto))", "min(-3.4e-2 - var(--foo), calc-size(auto))")]
    [InlineData("clamp(-10e3-var(--foo),calc-size(max-content),var(--foo)+-10e3)", "clamp(-10e3 - var(--foo), calc-size(max-content), var(--foo) + -10e3)")]
    public void DecodeArbitraryValue_PreservesCssKeywords(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    // A negative number immediately after `,` should not have spaces inserted
    [InlineData("clamp(-3px+4px,-3px+4px,-3px+4px)", "clamp(-3px + 4px, -3px + 4px, -3px + 4px)")]
    public void DecodeArbitraryValue_HandlesNegativesAfterComma(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    // Prevent formatting inside var() / env()
    [InlineData("calc(var(--foo-bar-bar)*2)", "calc(var(--foo-bar-bar) * 2)")]
    [InlineData("calc(env(safe-area-inset-bottom)*2)", "calc(env(safe-area-inset-bottom) * 2)")]
    public void DecodeArbitraryValue_DoesNotFormatInsideVarOrEnv(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    // Dashed functions that look like dashed idents
    [InlineData("fit-content(min(max-content,max(min-content,calc(20px+1em))))", "fit-content(min(max-content, max(min-content, calc(20px + 1em))))")]
    public void DecodeArbitraryValue_HandlesDashedFunctions(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    // Format inside calc() nested in env()
    [InlineData("env(safe-area-inset-bottom,calc(10px+20px))", "env(safe-area-inset-bottom,calc(10px + 20px))")]
    [InlineData("calc(env(safe-area-inset-bottom,calc(10px+20px))+5px)", "calc(env(safe-area-inset-bottom,calc(10px + 20px)) + 5px)")]
    public void DecodeArbitraryValue_FormatsCalcNestedInEnv(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    // Prevent formatting keywords like minmax(min-content,25%)
    [InlineData("minmax(min-content,25%)", "minmax(min-content,25%)")]
    [InlineData("radial-gradient(calc(1+2)),radial-gradient(calc(1+2))", "radial-gradient(calc(1 + 2)),radial-gradient(calc(1 + 2))")]
    public void DecodeArbitraryValue_PreservesNonMathFunctionKeywords(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    [InlineData("w-[calc(anchor-size(width)+8px)]", "w-[calc(anchor-size(width) + 8px)]")]
    [InlineData("w-[calc(anchor-size(foo(bar))+8px)]", "w-[calc(anchor-size(foo(bar)) + 8px)]")]
    public void DecodeArbitraryValue_HandlesAnchorSize(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    [InlineData("[content-start]_calc(100%-1px)_[content-end]_minmax(1rem,1fr)", "[content-start] calc(100% - 1px) [content-end] minmax(1rem,1fr)")]
    public void DecodeArbitraryValue_HandlesGridLineNamesAndMinmax(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    // round(...) function
    [InlineData("round(1+2,1+3)", "round(1 + 2, 1 + 3)")]
    [InlineData("round(to-zero,1+2,1+3)", "round(to-zero, 1 + 2, 1 + 3)")]
    public void DecodeArbitraryValue_HandlesRoundFunction(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    // Nested parens in non-math functions don't format their contents
    [InlineData("env((safe-area-inset-bottom))", "env((safe-area-inset-bottom))")]
    public void DecodeArbitraryValue_DoesNotFormatNestedParensInNonMathFunctions(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    [Theory]
    // -infinity is a keyword and should not have spaces around the -
    [InlineData("atan(1 + -infinity)", "atan(1 + -infinity)")]
    public void DecodeArbitraryValue_PreservesNegativeInfinityKeyword(string input, string expected) =>
        _parser.DecodeArbitraryValue(input).ShouldBe(expected);

    #endregion
}
