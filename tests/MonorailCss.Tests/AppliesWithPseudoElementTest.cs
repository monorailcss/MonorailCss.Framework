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

    [Fact]
    public void Process_WithPseudoElementKeyAndVariant_ComposesWithoutDoublingPseudoElement()
    {
        // Regression: a pseudo-element in the Applies KEY plus a variant utility produced a
        // doubled, malformed selector (".p-fetch-line::after::after:where(...)"). The variant
        // fragment must be inserted before the pseudo-element, which stays terminal.
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".p-fetch-line::after", "content-['→'] dark:text-red-300")
        };

        var framework = new CssFramework(settings);

        var result = framework.Process("");
        Console.WriteLine("Generated CSS:");
        Console.WriteLine(result);

        // Base rule keeps the pseudo-element from the key.
        result.ShouldContain(".p-fetch-line::after");
        result.ShouldContain("--tw-content: '→'");

        // Variant rule: :where(...) inserted before the (still terminal) pseudo-element.
        result.ShouldContain(".p-fetch-line:where(.dark, .dark *)::after");
        result.ShouldContain("color: var(--color-red-300)");

        // No doubled pseudo-element.
        result.ShouldNotContain("::after::after");
    }

    [Fact]
    public void Process_WithPseudoElementKeyAndPseudoClassVariant_KeepsPseudoElementLast()
    {
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".p-shard::before", "hover:text-red-500")
        };

        var framework = new CssFramework(settings);

        var result = framework.Process("");
        Console.WriteLine("Generated CSS:");
        Console.WriteLine(result);

        result.ShouldContain(".p-shard:hover::before");
        result.ShouldNotContain("::before::before");
        result.ShouldNotContain("::before:hover");
    }
}