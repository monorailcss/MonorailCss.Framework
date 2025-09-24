using System.Collections.Immutable;
using Shouldly;

namespace MonorailCss.Tests;

public class AppliesWithPseudoElementTest
{
    [Fact]
    public void Process_WithBeforeVariant_GeneratesCorrectPseudoElement()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".diff-add", "before:content-['+'] before:text-green-700 before:font-bold")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Output for debugging
        Console.WriteLine("Generated CSS:");
        Console.WriteLine(result);

        // Assert - verify the components layer exists
        result.ShouldContain("@layer components");

        // Base styles should be empty (all utilities have before: variant)
        result.ShouldContain(".diff-add {");

        // Before pseudo-element should use double colon
        result.ShouldContain(".diff-add::before");
        result.ShouldNotContain(".diff-add:before"); // Should NOT be single colon

        // Check the properties are applied
        result.ShouldContain("content: var(--tw-content)");
        result.ShouldContain("--tw-content: '+'");
        result.ShouldContain("color: var(--color-green-700)");
        result.ShouldContain("font-weight: var(--font-weight-bold)");
    }

    [Fact]
    public void Process_WithAfterVariant_GeneratesCorrectPseudoElement()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".tooltip", "relative after:content-['→'] after:absolute after:text-blue-500")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Output for debugging
        Console.WriteLine("Generated CSS:");
        Console.WriteLine(result);

        // Assert
        result.ShouldContain("@layer components");

        // Base styles should have relative
        result.ShouldContain(".tooltip {");
        result.ShouldContain("position: relative");

        // After pseudo-element should use double colon
        result.ShouldContain(".tooltip::after");
        result.ShouldNotContain(".tooltip:after"); // Should NOT be single colon

        // Check the properties are applied
        result.ShouldContain("content: var(--tw-content)");
        result.ShouldContain("--tw-content: '→'");
        result.ShouldContain("position: absolute");
        result.ShouldContain("color: var(--color-blue-500)");
    }
}