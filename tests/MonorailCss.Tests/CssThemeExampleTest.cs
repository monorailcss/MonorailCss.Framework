using System.Collections.Immutable;
using Shouldly;

namespace MonorailCss.Tests;

/// <summary>
/// Demonstrates the CSS theme configuration feature as requested by the user.
/// </summary>
public class CssThemeExampleTest
{
    [Fact]
    public void UserRequestedExample_WorksCorrectly()
    {
        // Arrange - exactly the example from the user request
        var cssSource = @"
            @import ""tailwindcss"";

            @theme {
                --color-orange-500: purple;
            }

            .btn {
                @apply bg-red-400 dark:bg-green-500 hover:bg-orange-500;
            }
        ";

        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            CssThemeSources = ImmutableList.Create(cssSource)
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Output for demonstration
        Console.WriteLine("=== Generated CSS ===");
        Console.WriteLine(result);
        Console.WriteLine("=====================");

        // Assert - verify key features work
        result.ShouldNotBeNullOrEmpty();

        // Theme override is applied
        result.ShouldContain("--color-orange-500: purple");

        // Component class is created
        result.ShouldContain(".btn");

        // Utilities are expanded
        result.ShouldContain("background-color");

        // Variants are handled
        result.ShouldContain("dark");
        result.ShouldContain("hover");
    }

    [Fact]
    public void CombinedCSharpAndCssTheme_WorksCorrectly()
    {
        // Arrange - combine C# theme with CSS theme as requested
        var csharpSettings = new CssFrameworkSettings
        {
            Theme = MonorailCss.Theme.Theme.CreateEmpty()
                .Add("--color-custom", "#123"),
            IncludePreflight = false
        };

        var cssSource = @"
            @import ""tailwindcss"";

            @theme {
                --color-orange-500: purple;
            }

            .btn {
                @apply bg-red-400 dark:bg-green-500 hover:bg-orange-500;
            }
        ";

        var finalSettings = csharpSettings with
        {
            CssThemeSources = ImmutableList.Create(cssSource)
        };

        var framework = new CssFramework(finalSettings);

        // Act
        var result = framework.Process("text-custom");

        // Output for demonstration
        Console.WriteLine("=== Combined C# and CSS Theme Output ===");
        Console.WriteLine(result);
        Console.WriteLine("=========================================");

        // Assert
        result.ShouldNotBeNullOrEmpty();

        // C# theme value is present
        result.ShouldContain("--color-custom: #123");

        // CSS theme override is applied
        result.ShouldContain("--color-orange-500: purple");

        // Component from CSS is created
        result.ShouldContain(".btn");

        // Utility class from process call works
        result.ShouldContain(".text-custom");
    }
}