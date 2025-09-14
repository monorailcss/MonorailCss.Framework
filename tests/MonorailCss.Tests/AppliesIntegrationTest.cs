using System.Collections.Immutable;
using Shouldly;

namespace MonorailCss.Tests;

public class AppliesIntegrationTest
{
    [Fact]
    public void Process_WithBtnApply_GeneratesExpectedOutput()
    {
        // Arrange - exactly as requested in the user's example
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "text-red-500 bg-blue-500")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Assert - verify the output structure matches the user's expectation
        result.ShouldContain("@layer theme, base, components, utilities;");

        // Theme layer with CSS variables
        result.ShouldContain("@layer theme");
        result.ShouldContain("--color-red-500: oklch(63.7% 0.237 25.331)");
        result.ShouldContain("--color-blue-500: oklch(62.3% 0.214 259.815)");

        // Components layer with .btn class
        result.ShouldContain("@layer components");
        result.ShouldContain(".btn");
        result.ShouldContain("background-color: var(--color-blue-500)");
        result.ShouldContain("color: var(--color-red-500)");

        // Output the result for verification
        Console.WriteLine("Generated CSS:");
        Console.WriteLine(result);
    }

    [Fact]
    public void Process_WithMultipleApplies_GeneratesExpectedOutput()
    {
        // Arrange - two component classes, both using text-red-500 to verify sharing works
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "text-red-500 bg-blue-500")
                .Add(".card", "text-red-500 bg-white shadow-lg")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Assert - verify the structure
        result.ShouldContain("@layer theme, base, components, utilities;");

        // Theme layer should have all required variables (red-500 shared by both)
        result.ShouldContain("@layer theme");
        result.ShouldContain("--color-red-500: oklch(63.7% 0.237 25.331)");
        result.ShouldContain("--color-blue-500: oklch(62.3% 0.214 259.815)");
        result.ShouldContain("--color-white"); // Just check the variable exists

        // Components layer should have both classes
        result.ShouldContain("@layer components");
        result.ShouldContain(".btn");
        result.ShouldContain(".card");

        // Verify .btn properties
        var btnIndex = result.IndexOf(".btn", StringComparison.Ordinal);
        var cardIndex = result.IndexOf(".card", StringComparison.Ordinal);
        btnIndex.ShouldBeGreaterThan(0);
        cardIndex.ShouldBeGreaterThan(0);

        // Both should have text-red-500 color
        result.ShouldContain("color: var(--color-red-500)");

        // .btn should have blue background
        result.ShouldContain("background-color: var(--color-blue-500)");

        // .card should have white background
        result.ShouldContain("background-color: var(--color-white)");

        // Output for verification
        Console.WriteLine("Generated CSS with multiple applies:");
        Console.WriteLine(result);
    }

    [Fact]
    public void Process_WithVariants_GeneratesExpectedOutput()
    {
        // Arrange - two component classes, both using text-red-500 to verify sharing works
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "text-red-500 bg-blue-500 dark:bg-green-500")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Assert - verify the structure

        Console.WriteLine("Generated CSS with variants:");
        Console.WriteLine(result);

        result.ShouldContain("@layer theme, base, components, utilities;");

        // Theme layer should have all required variables (red-500 shared by both)
        result.ShouldContain("@layer theme");
        result.ShouldContain("--color-red-500: oklch(63.7% 0.237 25.331)");
        result.ShouldContain("--color-blue-500: oklch(62.3% 0.214 259.815)");
        result.ShouldContain("--color-green-500: oklch(72.3% 0.219 149.579);");

        // Components layer should have both classes
        result.ShouldContain("@layer components");
        result.ShouldContain(".btn");

        // Check for the correct dark variant selector structure
        // Should be :where(.dark, .dark *) .btn (descendant selector)
        result.ShouldContain(":where(.dark, .dark *) .btn");

    }

    [Fact]
    public void Process_WithVariants_GeneratesCorrectSelectorStructure()
    {
        // Arrange - verify the exact CSS selector structure for variants
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "bg-blue-500 hover:bg-red-500 dark:bg-green-500")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Output for debugging
        Console.WriteLine("Generated CSS:");
        Console.WriteLine(result);

        // Assert - verify the components layer exists
        result.ShouldContain("@layer components");

        // Check for base .btn styles
        result.ShouldContain(".btn {");
        result.ShouldContain("background-color: var(--color-blue-500)");

        // Hover variant - should be combined selector, not nested
        result.ShouldContain(".btn:hover");
        result.ShouldContain("background-color: var(--color-red-500)");

        // Dark variant - should use descendant selector for applies
        result.ShouldContain(":where(.dark, .dark *) .btn");
        result.ShouldContain("background-color: var(--color-green-500)");
    }
}