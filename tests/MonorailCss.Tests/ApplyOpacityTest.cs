using System.Collections.Immutable;
using Shouldly;

namespace MonorailCss.Tests;

public class ApplyOpacityTest
{
    [Fact]
    public void Applies_ShouldHandleOpacityModifiers()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".test-component", "bg-red-100/25 border-red-600")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Debug - print the result to see what's generated
        Console.WriteLine("Generated CSS:");
        Console.WriteLine(result);

        // Assert
        result.ShouldContain("background-color: color-mix(in oklab,");
        result.ShouldContain("25%");
        result.ShouldContain(".test-component");
    }

    [Fact]
    public void Applies_ShouldHandleDarkVariantWithOpacity()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".highlight", "border-red-600 dark:border-red-300/25")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");
        Console.WriteLine(result);

        // Assert
        // Base rule
        result.ShouldContain(".highlight");
        result.ShouldContain("border-color:");

        // Dark variant with opacity
        result.ShouldContain(":where(.dark, .dark *)");
        result.ShouldContain("color-mix(in oklab,");
        result.ShouldContain("25%");
    }

    [Fact]
    public void Applies_ComplexExample_ShouldWork()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".code-highlight-wrapper .word-highlight-with-message",
                     "border border-b border-primary-600 dark:border-primary-red/25 rounded px-1 py-1 bg-red-100/25 dark:bg-red-500/10 relative")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");
        Console.WriteLine(result);

        // Assert
        result.ShouldContain(".code-highlight-wrapper .word-highlight-with-message");

        // Check for opacity modifiers being applied
        result.ShouldContain("background-color: color-mix(in oklab,");
        result.ShouldContain("25%"); // bg-primary-100/25

        // Check for dark variant
        result.ShouldContain(":where(.dark, .dark *)");
        result.ShouldContain("10%"); // dark:bg-primary-500/10
    }
}