using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Shouldly;

namespace MonorailCss.Tests;

/// <summary>
/// End-to-end integration tests for the CssFramework.
/// These tests verify the complete pipeline from input classes to generated CSS.
/// </summary>
public partial class CssFrameworkTests
{
    private readonly CssFramework _framework;

    public CssFrameworkTests()
    {
        _framework = new CssFramework();
    }

    [Fact]
    public void Process_ShouldHandleEmptyInput()
    {
        var result = _framework.Process("");
        result.ShouldBe("");

        var detailed = _framework.ProcessWithDetails("");
        detailed.ProcessedClasses.ShouldBeEmpty();
        detailed.InvalidClasses.ShouldBeEmpty();
        detailed.GeneratedCss.ShouldBe("");
    }

    [Fact]
    public void Process_ShouldHandleNullInput()
    {
        var result = _framework.Process((string)null!);
        result.ShouldBe("");

        var detailed = _framework.ProcessWithDetails((string)null!);
        detailed.ProcessedClasses.ShouldBeEmpty();
        detailed.InvalidClasses.ShouldBeEmpty();
        detailed.GeneratedCss.ShouldBe("");
    }

    [Fact]
    public void Process_ShouldHandleDuplicates()
    {
        var detailed = _framework.ProcessWithDetails("mb-4 mb-4");
        detailed.ProcessedClasses.ShouldHaveSingleItem();
        detailed.ProcessedClasses.First().ClassName.ShouldBe("mb-4");
    }

    [Fact]
    public void Process_ShouldGenerateDisplayUtilities()
    {
        var testCases = new[]
        {
            ("block", "display: block"),
            ("inline", "display: inline"),
            ("flex", "display: flex"),
            ("grid", "display: grid"),
            ("hidden", "display: none"),
            ("inline-block", "display: inline-block"),
            ("inline-flex", "display: inline-flex"),
            ("inline-grid", "display: inline-grid")
        };

        foreach (var (input, expected) in testCases)
        {
            var css = _framework.Process(input);
            css.ShouldContain(expected);
            css.ShouldContain($".{input}");
        }
    }

    [Fact]
    public void Process_ShouldGenerateBackgroundColorUtilities()
    {
        var css = _framework.Process("bg-red-500");

        css.ShouldContain("--color-red-500");
        css.ShouldContain("background-color: var(--color-red-500)");
        css.ShouldContain(".bg-red-500");
    }

    [Fact]
    public void Process_ShouldGenerateTextColorUtilities()
    {
        var css = _framework.Process("text-blue-700");

        css.ShouldContain("--color-blue-700");
        css.ShouldContain("color: var(--color-blue-700)");
        css.ShouldContain(".text-blue-700");
    }

    [Fact]
    public void Process_ShouldHandleSpecialColorValues()
    {
        var testCases = new[]
        {
            ("bg-transparent", "background-color: transparent"),
            ("text-current", "color: currentColor"),
            ("bg-inherit", "background-color: inherit")
        };

        foreach (var (input, expected) in testCases)
        {
            var css = _framework.Process(input);
            css.ShouldContain(expected);
        }
    }

    [Fact]
    public void Process_ShouldHandleArbitraryColorValues()
    {
        var testCases = new[]
        {
            ("bg-[#123456]", "background-color: #123456"),
            ("text-[rgb(255,0,0)]", "color: rgb(255,0,0)"),
            ("bg-[hsl(0,100%,50%)]", "background-color: hsl(0,100%,50%)")
        };

        foreach (var (input, expected) in testCases)
        {
            var css = _framework.Process(input);
            css.ShouldContain(expected);
            css.ShouldContain($".{input.Replace("[", "\\[").Replace("]", "\\]").Replace(",", "\\,").Replace("(", "\\(").Replace(")", "\\)").Replace("%", "\\%").Replace("#", "\\#")}");
        }
    }

    [Fact]
    public void Process_ShouldHandleOpacityModifiers()
    {
        var testCases = new[]
        {
            ("bg-red-500/50", "color-mix(in oklab, var(--color-red-500) 50%, transparent)"),
            ("text-blue-700/25", "color-mix(in oklab, var(--color-blue-700) 25%, transparent)"),
            ("bg-green-400/[0.75]", "color-mix(in oklab, var(--color-green-400) 75%, transparent)")
        };

        foreach (var (input, expected) in testCases)
        {
            var css = _framework.Process(input);
            css.ShouldContain(expected);
        }
    }

    [Fact]
    public void Process_ShouldHandleHoverVariant()
    {
        var css = _framework.Process("hover:bg-red-500");

        css.ShouldContain(".hover\\:bg-red-500:hover");
        css.ShouldContain("background-color: var(--color-red-500)");
    }

    [Fact]
    public void Process_ShouldHandleFocusVariant()
    {
        var css = _framework.Process("focus:text-blue-700");

        css.ShouldContain(".focus\\:text-blue-700:focus");
        css.ShouldContain("color: var(--color-blue-700)");
    }

    [Fact]
    public void Process_ShouldHandleActiveVariant()
    {
        var css = _framework.Process("active:bg-green-500");

        css.ShouldContain(".active\\:bg-green-500:active");
        css.ShouldContain("background-color: var(--color-green-500)");
    }

    [Fact]
    public void Process_ShouldHandleMultipleVariants()
    {
        var css = _framework.Process("hover:focus:bg-red-500");

        css.ShouldContain(".hover\\:focus\\:bg-red-500:hover:focus");
        css.ShouldContain("background-color: var(--color-red-500)");
    }

    [Fact]
    public void Process_ShouldHandleImportantModifier()
    {
        var testCases = new[]
        {
            "!bg-red-500",
            "bg-red-500!"
        };

        foreach (var input in testCases)
        {
            var css = _framework.Process(input);
            css.ShouldContain("background-color: var(--color-red-500) !important");
        }
    }

    [Fact]
    public void Process_ShouldHandleImportantWithVariants()
    {
        var css = _framework.Process("hover:!bg-red-500");

        css.ShouldContain(".hover\\:\\!bg-red-500:hover");
        css.ShouldContain("background-color: var(--color-red-500) !important");
    }

    [Fact]
    public void Process_ShouldHandleMultipleClasses()
    {
        var css = _framework.Process("bg-red-500 text-white p-4 flex");

        // Check all utilities are processed
        css.ShouldContain("background-color: var(--color-red-500)");
        css.ShouldContain("color: var(--color-white)");
        css.ShouldContain("padding: calc(var(--spacing) * 4)");
        css.ShouldContain("display: flex");

        // Check theme variables
        css.ShouldContain("--color-red-500");
        css.ShouldContain("--color-white");
        css.ShouldContain("--spacing");
    }

    [Fact]
    public void Process_ShouldTrackInvalidClasses()
    {
        var result = _framework.ProcessWithDetails("bg-red-500 invalid-class text-white unknown-utility");

        result.ProcessedClasses.Length.ShouldBe(2);
        result.InvalidClasses.Length.ShouldBe(2);
        result.InvalidClasses.ShouldContain("invalid-class");
        result.InvalidClasses.ShouldContain("unknown-utility");
    }

    [Fact]
    public void Process_ShouldGenerateLayeredOutput()
    {
        var css = _framework.Process("bg-red-500 p-4");

        // Should have theme layer with variables
        css.ShouldContain("@layer theme");
        css.ShouldContain("--color-red-500");
        css.ShouldContain("--spacing");

        // Should have utilities layer with rules
        css.ShouldContain("@layer utilities");
        css.ShouldContain(".bg-red-500");
        css.ShouldContain(".p-4");

        // Theme layer should come before utilities
        var themeIndex = css.IndexOf("@layer theme", StringComparison.Ordinal);
        var utilitiesIndex = css.IndexOf("@layer utilities", StringComparison.Ordinal);
        themeIndex.ShouldBeLessThan(utilitiesIndex);
    }

    [Fact]
    public void Process_ShouldOnlyIncludeUsedThemeVariables()
    {
        var css = _framework.Process("bg-red-500");

        // Should include red-500
        css.ShouldContain("--color-red-500");

        // Should NOT include unused colors
        css.ShouldNotContain("--color-blue-500");
        css.ShouldNotContain("--color-green-500");
        css.ShouldNotContain("--color-yellow-500");
    }

    [Fact]
    public void Process_ShouldHandleSpacingUtilities()
    {
        var css = _framework.Process("p-4 m-2 space-x-4 space-y-2");

        // Check padding
        css.ShouldContain(".p-4");
        css.ShouldContain("padding: calc(var(--spacing) * 4)");

        // Check margin
        css.ShouldContain(".m-2");
        css.ShouldContain("margin: calc(var(--spacing) * 2)");

        // Check space utilities
        css.ShouldContain(".space-x-4");
        css.ShouldContain(".space-y-2");
        css.ShouldContain("--tw-space-x-reverse");
        css.ShouldContain("--tw-space-y-reverse");
    }

    [Fact]
    public void Process_ShouldHandleComplexClassCombinations()
    {
        var css = _framework.Process("hover:bg-red-500/50 focus:!text-blue-700 active:p-4 dark:bg-gray-900");

        // Verify each combination is processed
        css.ShouldContain(".hover\\:bg-red-500\\/50:hover");
        css.ShouldContain("color-mix(in oklab, var(--color-red-500) 50%, transparent)");

        css.ShouldContain(".focus\\:\\!text-blue-700:focus");
        css.ShouldContain("color: var(--color-blue-700) !important");

        css.ShouldContain(".active\\:p-4:active");
        css.ShouldContain("padding: calc(var(--spacing) * 4)");

        css.ShouldContain(".dark\\:bg-gray-900:where(.dark, .dark *)");
        css.ShouldContain("background-color: var(--color-gray-900)");
    }

    [Fact]
    public void ProcessMultiple_ShouldCombineMultipleInputs()
    {
        var css = _framework.ProcessMultiple("bg-red-500", "text-white", "p-4");

        css.ShouldContain("background-color: var(--color-red-500)");
        css.ShouldContain("color: var(--color-white)");
        css.ShouldContain("padding: calc(var(--spacing) * 4)");
    }

    [Fact]
    public void ProcessMultiple_ShouldIgnoreEmptyStrings()
    {
        var css = _framework.ProcessMultiple("bg-red-500", "", "text-white", null!, "p-4");

        css.ShouldContain("background-color: var(--color-red-500)");
        css.ShouldContain("color: var(--color-white)");
        css.ShouldContain("padding: calc(var(--spacing) * 4)");
    }

    [Fact]
    public void ProcessWithDetails_ShouldProvideDetailedResults()
    {
        var result = _framework.ProcessWithDetails("bg-red-500 text-white invalid-class");

        result.Input.ShouldBe("bg-red-500 text-white invalid-class");
        result.TotalClasses.ShouldBe(3);
        result.ProcessedClasses.Length.ShouldBe(2);
        result.InvalidClasses.Length.ShouldBe(1);
        result.SuccessRate.ShouldBe(2.0 / 3.0);
        result.IsFullyProcessed.ShouldBeFalse();
        result.HasProcessedClasses.ShouldBeTrue();
        result.HasInvalidClasses.ShouldBeTrue();

        // Check processed class details
        var bgClass = result.ProcessedClasses.FirstOrDefault(p => p.ClassName == "bg-red-500");
        bgClass.ShouldNotBeNull();
        bgClass.UtilityName.ShouldBe("BackgroundColorUtility");
        bgClass.AstNodes.ShouldNotBeEmpty();

        var textClass = result.ProcessedClasses.FirstOrDefault(p => p.ClassName == "text-white");
        textClass.ShouldNotBeNull();
        textClass.UtilityName.ShouldBe("TextUtility");
        textClass.AstNodes.ShouldNotBeEmpty();
    }

    [Fact]
    public void Process_ShouldMaintainCorrectCssOrder()
    {
        // According to Tailwind's canonical ordering, display utilities should come before colors
        var css = _framework.Process("text-white bg-red-500 flex p-4");

        // Extract utility class positions
        var flexIndex = css.IndexOf(".flex", StringComparison.Ordinal);
        var paddingIndex = css.IndexOf(".p-4", StringComparison.Ordinal);
        var bgIndex = css.IndexOf(".bg-red-500", StringComparison.Ordinal);
        var textIndex = css.IndexOf(".text-white", StringComparison.Ordinal);

        // Verify ordering (display -> backgrounds -> spacing -> text)
        flexIndex.ShouldBeLessThan(bgIndex);
        bgIndex.ShouldBeLessThan(paddingIndex);
        paddingIndex.ShouldBeLessThan(textIndex);
    }

    [Fact]
    public void Process_ShouldHandleBreakpointVariants()
    {
        var css = _framework.Process("sm:p-4 md:p-6 lg:p-8");

        css.ShouldContain("@media (min-width: 640px)");
        css.ShouldContain(".sm\\:p-4");

        css.ShouldContain("@media (min-width: 768px)");
        css.ShouldContain(".md\\:p-6");

        css.ShouldContain("@media (min-width: 1024px)");
        css.ShouldContain(".lg\\:p-8");
    }

    [Fact]
    public void Process_ShouldHandleGroupAndPeerVariants()
    {
        var css = _framework.Process("group-hover:bg-red-500 peer-focus:text-blue-700");

        css.ShouldContain(".group-hover\\:bg-red-500:is(:where(.group):hover *)");
        css.ShouldContain(".peer-focus\\:text-blue-700:is(:where(.peer):focus ~ *)");
    }

    [Fact]
    public void Process_ShouldGenerateValidCssOutput()
    {
        var css = _framework.Process("bg-red-500 hover:bg-red-600 text-white p-4 rounded-lg shadow-md");

        // Should be valid CSS with proper structure
        css.ShouldContain("@layer theme");
        css.ShouldContain("@layer utilities");
        css.ShouldContain(":root");
        css.ShouldContain("{");
        css.ShouldContain("}");

        // All CSS property declarations should end with semicolons
        var lines = css.Split('\n');
        var inComment = false;
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Track comment blocks
            if (trimmedLine.Contains("/*"))
                inComment = true;
            if (trimmedLine.Contains("*/"))
                inComment = false;

            // Check for CSS property declarations (property: value)
            // Skip selectors (which may contain : or ::), at-rules, and comments
            if (!inComment && line.Contains(':') && !trimmedLine.StartsWith("/*"))
            {
                // CSS property declarations have the format "property: value"
                // They don't start with @, don't end with {, and don't contain :: (pseudo-elements)
                var colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    var beforeColon = line[..colonIndex].Trim();
                    var isProperty = !beforeColon.StartsWith("@") &&
                                   !beforeColon.Contains("::") &&
                                   !line.TrimEnd().EndsWith("{") &&
                                   !line.TrimEnd().EndsWith(",") &&
                                   !line.Contains("@layer") &&
                                   !line.Contains("@media") &&
                                   !line.Contains("@supports") &&
                                   !line.Contains("@import");

                    if (isProperty && beforeColon.All(c => char.IsLetterOrDigit(c) || c == '-' || c == ' '))
                    {
                        // Remove inline comments before checking for semicolon
                        var lineWithoutComment = line;
                        var commentStart = line.IndexOf("/*", StringComparison.Ordinal);
                        if (commentStart > 0)
                        {
                            lineWithoutComment = line[..commentStart];
                        }
                        lineWithoutComment.TrimEnd().ShouldEndWith(";");
                    }
                }
            }
        }
    }

    [Fact]
    public void Process_ShouldHandleCustomColorPalette()
    {
        // Arrange - Create a theme with a custom brand color palette
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

        var customTheme = new MonorailCss.Theme.Theme()
            .AddColorPalette("brand", brandColors.ToImmutable());

        var customSettings = new CssFrameworkSettings { Theme = customTheme };
        var customFramework = new CssFramework(customSettings);

        // Act
        var css = customFramework.Process("bg-brand-500 text-brand-100 hover:bg-brand-600");

        // Assert - Verify custom colors are recognized and processed
        css.ShouldContain("--color-brand-500: oklch(71.2% 0.191 280)");
        css.ShouldContain("--color-brand-100: oklch(96.3% 0.024 280)");
        css.ShouldContain("--color-brand-600: oklch(62.8% 0.208 280)");

        css.ShouldContain(".bg-brand-500");
        css.ShouldContain("background-color: var(--color-brand-500)");

        css.ShouldContain(".text-brand-100");
        css.ShouldContain("color: var(--color-brand-100)");

        css.ShouldContain(".hover\\:bg-brand-600:hover");
        css.ShouldContain("background-color: var(--color-brand-600)");
    }

    [Fact]
    public void Process_ShouldHandleCustomFontFamily()
    {
        // Arrange - Create a theme with custom font families
        var customTheme = new MonorailCss.Theme.Theme()
            .AddFontFamily("sans", "'Inter', sans-serif") // override
            .AddFontFamily("mono", "'Fira Code', monospace") // override
            .AddFontFamily("display", "'Playfair Display', serif"); // custom


        var customSettings = new CssFrameworkSettings { Theme = customTheme };
        var customFramework = new CssFramework(customSettings);

        // Act
        var css = customFramework.Process("font-display font-sans font-mono");

        // Assert - Verify custom font families are recognized and processed
        css.ShouldContain("--font-display: 'Playfair Display', serif");
        css.ShouldContain("--font-sans: 'Inter', sans-serif");
        css.ShouldContain("--font-mono: 'Fira Code', monospace");

        css.ShouldContain(".font-display");
        css.ShouldContain("font-family: var(--font-display)");

        css.ShouldContain(".font-sans");
        css.ShouldContain("font-family: var(--font-sans)");

        css.ShouldContain(".font-mono");
        css.ShouldContain("font-family: var(--font-mono)");
    }

    [Fact]
    public void Process_ShouldHandleCustomColorWithOpacityModifier()
    {
        // Arrange - Create a theme with a custom accent color
        var accentColors = ImmutableDictionary.CreateBuilder<string, string>();
        accentColors["500"] = "oklch(65% 0.2 320)";

        var customTheme = new MonorailCss.Theme.Theme()
            .AddColorPalette("accent", accentColors.ToImmutable());

        var customSettings = new CssFrameworkSettings { Theme = customTheme };
        var customFramework = new CssFramework(customSettings);

        // Act
        var css = customFramework.Process("bg-accent-500/50 text-accent-500/25");

        // Assert - Verify opacity modifiers work with custom colors
        css.ShouldContain("--color-accent-500: oklch(65% 0.2 320)");

        css.ShouldContain(".bg-accent-500\\/50");
        css.ShouldContain("background-color: color-mix(in oklab, var(--color-accent-500) 50%, transparent)");

        css.ShouldContain(".text-accent-500\\/25");
        css.ShouldContain("color: color-mix(in oklab, var(--color-accent-500) 25%, transparent)");
    }

    [Fact]
    public void ProcessWithDetails_ShouldReportCustomClassesAsValid()
    {
        // Arrange - Create a theme with custom colors
        var brandColors = ImmutableDictionary.CreateBuilder<string, string>();
        brandColors["500"] = "#FF6B6B";

        var customTheme = new MonorailCss.Theme.Theme()
            .AddColorPalette("brand", brandColors.ToImmutable())
            .AddFontFamily("custom", "'Custom Font', sans-serif");

        var customSettings = new CssFrameworkSettings { Theme = customTheme };
        var customFramework = new CssFramework(customSettings);

        // Act
        var result = customFramework.ProcessWithDetails("bg-brand-500 font-custom invalid-utility");

        // Assert - Custom classes should be processed, not marked as invalid
        result.ProcessedClasses.Length.ShouldBe(2);
        result.InvalidClasses.Length.ShouldBe(1);

        result.ProcessedClasses.ShouldContain(p => p.ClassName == "bg-brand-500");
        result.ProcessedClasses.ShouldContain(p => p.ClassName == "font-custom");
        result.InvalidClasses.ShouldContain("invalid-utility");

        result.IsFullyProcessed.ShouldBeFalse();
        result.SuccessRate.ShouldBe(2.0 / 3.0);
    }

    [Fact]
    public void Process_ShouldHandleVarFunctionsInArbitraryValues()
    {
        // Act
        var css = _framework.Process("h-[var(--navbar-height)] w-[var(--sidebar-width)]");

        // Assert - Verify var() functions are preserved in CSS output
        css.ShouldContain(".h-\\[var\\(--navbar-height\\)\\]");
        css.ShouldContain("height: var(--navbar-height)");

        css.ShouldContain(".w-\\[var\\(--sidebar-width\\)\\]");
        css.ShouldContain("width: var(--sidebar-width)");
    }

    [Fact]
    public void Process_ShouldHandleComplexCssFunctionsInArbitraryValues()
    {
        // Act
        var css = _framework.Process("h-[calc(100vh-var(--navbar-height))] max-w-[min(100%,var(--max-width))] min-h-[clamp(10rem,var(--size),20rem)]");

        // Assert - Verify complex CSS functions with var() are processed correctly
        css.ShouldContain(".h-\\[calc\\(100vh-var\\(--navbar-height\\)\\)\\]");
        css.ShouldContain("height: calc(100vh - var(--navbar-height))");

        css.ShouldContain(".max-w-\\[min\\(100\\%\\,var\\(--max-width\\)\\)\\]");
        css.ShouldContain("max-width: min(100%, var(--max-width))");

        css.ShouldContain(".min-h-\\[clamp\\(10rem\\,var\\(--size\\)\\,20rem\\)\\]");
        css.ShouldContain("min-height: clamp(10rem, var(--size), 20rem)");
    }

    [Fact]
    public void Process_ShouldHandleVarFunctionsWithFallbacks()
    {
        // Act
        var css = _framework.Process("h-[var(--navbar-height,64px)] w-[var(--sidebar-width,200px)]");

        // Assert - Verify var() functions with fallbacks are preserved
        css.ShouldContain(".h-\\[var\\(--navbar-height\\,64px\\)\\]");
        css.ShouldContain("height: var(--navbar-height,64px)");

        css.ShouldContain(".w-\\[var\\(--sidebar-width\\,200px\\)\\]");
        css.ShouldContain("width: var(--sidebar-width,200px)");
    }
}