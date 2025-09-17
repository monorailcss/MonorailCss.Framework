using System.Collections.Immutable;
using Shouldly;

namespace MonorailCss.Tests;

public class CssThemeIntegrationTests
{
    [Fact]
    public void Process_WithCssThemeSource_AppliesThemeVariables()
    {
        // Arrange
        var cssSource = """

                        @import "tailwindcss";

                        @theme {
                            --color-orange-500: purple;
                            --color-custom: #123456;
                        }

                        """;

        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            CssThemeSources = ImmutableList.Create(cssSource)
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("bg-orange-500 text-custom");
        Console.WriteLine(result);

        // Assert
        result.ShouldContain("--color-orange-500: purple");
        result.ShouldContain("--color-custom: #123456");
        result.ShouldContain("background-color: var(--color-orange-500)");
        result.ShouldContain("color: var(--color-custom)");
    }

    [Fact]
    public void Process_WithCssComponentRules_GeneratesComponentClasses()
    {
        // Arrange
        var cssSource = """

                        .btn {
                            @apply bg-red-400 text-white p-4;
                        }

                        .card {
                            @apply shadow-lg rounded-lg;
                        }

                        """;

        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            CssThemeSources = ImmutableList.Create(cssSource)
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");
        Console.WriteLine(result);

        // Assert
        result.ShouldContain("@layer components");
        result.ShouldContain(".btn");
        result.ShouldContain(".card");
        result.ShouldContain("background-color: var(--color-red-400)");
        result.ShouldContain("color: var(--color-white)");
        result.ShouldContain("padding: calc(var(--spacing) * 4)");
    }

    [Fact]
    public void Process_WithCssComponentWithVariants_HandlesVariantsCorrectly()
    {
        // Arrange
        var cssSource = """

                        .btn {
                            @apply bg-red-400 dark:bg-green-500 hover:bg-orange-500;
                        }

                        """;

        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            CssThemeSources = ImmutableList.Create(cssSource)
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");
        Console.WriteLine(result);

        // Assert
        result.ShouldContain("@layer components");
        result.ShouldContain(".btn");
        result.ShouldContain("background-color: var(--color-red-400)");
        result.ShouldContain(".btn:where(.dark, .dark *)");
        result.ShouldContain("background-color: var(--color-green-500)");
    }

    [Fact]
    public void Process_CombiningCSharpAndCssThemes_MergesCorrectly()
    {
        // Arrange - C# theme first
        var csharpTheme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-blue-500", "#3b82f6")
            .Add("--color-red-500", "#ef4444");

        // CSS theme overrides red and adds green
        var cssSource = """

                        @theme {
                            --color-red-500: crimson;
                            --color-green-500: #10b981;
                        }

                        """;

        var settings = new CssFrameworkSettings
        {
            Theme = csharpTheme,
            IncludePreflight = false,
            CssThemeSources = ImmutableList.Create(cssSource)
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("bg-blue-500 text-red-500 border-green-500");
        Console.WriteLine(result);

        // Assert
        result.ShouldContain("--color-blue-500: #3b82f6"); // From C#
        result.ShouldContain("--color-red-500: crimson"); // Overridden by CSS
        result.ShouldContain("--color-green-500: #10b981"); // From CSS
    }

    [Fact]
    public void Process_CombiningCSharpAndCssApplies_MergesCorrectly()
    {
        // Arrange
        var csharpApplies = ImmutableDictionary<string, string>.Empty
            .Add(".btn-primary", "bg-blue-500 text-white")
            .Add(".card", "shadow-md p-4");

        var cssSource = """

                        .btn-secondary {
                            @apply bg-gray-500 text-white;
                        }

                        .card {
                            @apply shadow-lg rounded-lg p-6;
                        }

                        """;

        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = csharpApplies,
            CssThemeSources = ImmutableList.Create(cssSource)
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");
        Console.WriteLine(result);

        // Assert
        result.ShouldContain(".btn-primary"); // From C#
        result.ShouldContain(".btn-secondary"); // From CSS
        result.ShouldContain(".card"); // Overridden by CSS

        // Check that .card uses CSS version (shadow-lg instead of shadow-md, p-6 instead of p-4)
        var cardSection = result.Substring(result.IndexOf(".card", StringComparison.Ordinal));
        cardSection.ShouldContain("--tw-shadow: 0 10px 15px"); // shadow-lg
        cardSection.ShouldContain("padding: calc(var(--spacing) * 6)"); // p-6
    }

    [Fact]
    public void Process_MultipleCssSources_ProcessesInOrder()
    {
        // Arrange
        var cssSource1 = """

                         @theme {
                             --color-primary: blue;
                             --color-secondary: gray;
                         }

                         .btn {
                             @apply bg-blue-500;
                         }

                         """;

        var cssSource2 = """

                         @theme {
                             --color-primary: green;
                             --color-tertiary: purple;
                         }

                         .btn {
                             @apply bg-green-500 text-white;
                         }

                         .link {
                             @apply text-blue-500 underline;
                         }

                         """;

        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            CssThemeSources = ImmutableList.Create(cssSource1, cssSource2)
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");
        Console.WriteLine(result);

        // Assert
        // Note: Custom theme variables that aren't used by utilities won't appear in output
        // Only variables actually referenced by utilities are included

        // Components - second source wins for .btn
        result.ShouldContain(".btn");
        result.ShouldContain(".link");
        var btnSection = result.Substring(result.IndexOf(".btn", StringComparison.Ordinal));
        var linkIndex = result.IndexOf(".link", StringComparison.Ordinal);
        if (linkIndex > 0 && linkIndex < btnSection.Length)
        {
            btnSection = btnSection.Substring(0, linkIndex);
        }
        btnSection.ShouldContain("background-color: var(--color-green-500)");
        btnSection.ShouldContain("color: var(--color-white)");
    }

    [Fact]
    public void Process_FullExampleFromUserRequest_WorksCorrectly()
    {
        // Arrange - exactly as requested by the user
        var cssSource = """

                        @import "tailwindcss";

                        @theme {
                            --color-orange-500: purple;
                        }

                        .btn {
                            @apply bg-red-400 dark:bg-green-500 hover:bg-orange-500;
                        }

                        """;

        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            CssThemeSources = ImmutableList.Create(cssSource)
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");
        Console.WriteLine(result);

        // Assert
        result.ShouldContain("@layer theme");
        result.ShouldContain("@layer components");

        // Theme variable override
        result.ShouldContain("--color-orange-500: purple");

        // Component with variants
        result.ShouldContain(".btn");
        result.ShouldContain("background-color: var(--color-red-400)");

        // Dark variant
        result.ShouldContain(".btn:where(.dark, .dark *)");

        // The hover variant should use the overridden orange color
        result.ShouldContain(".btn:hover");
        result.ShouldContain("background-color: var(--color-orange-500)");
    }

    [Fact]
    public void Process_ChainedThemeConfiguration_WorksInOrder()
    {
        // Arrange - hardcoded theme + CSS + more code as mentioned by user
        var hardcodedTheme = MonorailCss.Theme.Theme.CreateEmpty()
            .Add("--color-blue-500", "#3b82f6")
            .Add("--spacing-4", "1rem");

        var cssSource = """

                        @theme {
                            --color-blue-500: navy;
                            --color-red-500: crimson;
                        }

                        """;

        // After CSS processing, add more through C# (simulated by another CSS source)
        var additionalCssSource = """

                                  @theme {
                                      --color-green-500: #10b981;
                                      --color-red-500: darkred;
                                  }

                                  """;

        var settings = new CssFrameworkSettings
        {
            Theme = hardcodedTheme,
            IncludePreflight = false,
            CssThemeSources = ImmutableList.Create(cssSource, additionalCssSource)
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("bg-blue-500 text-red-500 border-green-500 p-4");
        Console.WriteLine(result);

        // Assert
        result.ShouldContain("--color-blue-500: navy"); // Overridden by first CSS
        result.ShouldContain("--color-red-500: darkred"); // Overridden by second CSS
        result.ShouldContain("--color-green-500: #10b981"); // Added by second CSS
        // Note: --spacing-4 won't appear directly, MonorailCss uses calc(var(--spacing) * 4) instead
    }
}