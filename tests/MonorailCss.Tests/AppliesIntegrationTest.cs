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
        // Should be .btn:where(.dark, .dark *) matching Tailwind's behavior
        result.ShouldContain(".btn:where(.dark, .dark *)");

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

        // Dark variant - should use :where() pseudo-class selector
        result.ShouldContain(".btn:where(.dark, .dark *)");
        result.ShouldContain("background-color: var(--color-green-500)");
    }

    [Fact]
    public void Process_WithBeforeContentAndMediaQuery_GeneratesCorrectStructure()
    {
        // Arrange - Test the exact bug scenario reported
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "before:content-['+'] before:hidden md:before:block")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        Console.WriteLine(result);

        // Assert - verify the components layer exists
        result.ShouldContain("@layer components");

        // Should have base .btn rule (can be empty)
        result.ShouldContain(".btn");

        // Should have .btn::before with content and display:none
        result.ShouldContain(".btn::before");

        // The before pseudo-element should have both content declarations and display: none
        // We need to check that the content property is properly set
        result.ShouldContain("--tw-content: '+'");
        result.ShouldContain("content: var(--tw-content)");
        result.ShouldContain("display: none");

        // Media query variant should wrap the rule, not modify the selector
        result.ShouldContain("@media (min-width: 768px)");
        result.ShouldContain("display: block");

        // Should NOT have invalid syntax like .btn:md::before
        result.ShouldNotContain(".btn:md::before");
        result.ShouldNotContain(":md");
    }


    [Fact]
    public void Process_WithDataVariant_GeneratesCorrectStructure()
    {
        // Arrange - Test the exact bug scenario reported
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "data-[selected=true]:text-red-400")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        Console.WriteLine(result);

        // Assert - verify the components layer exists
        result.ShouldContain("@layer components");

        // Should have base .btn rule (can be empty)
        result.ShouldContain(".btn");

        // Should have data-selected = "true"
        result.ShouldContain(".btn[data-selected=\"true\"]");

        // and it should be red
        result.ShouldContain("var(--color-red-400);");
    }

    public static IEnumerable<object[]> VariantTestData()
    {
        // Pseudo-class variants
        yield return [".btn", "hover:bg-red-500", ".btn:hover", "background-color: var(--color-red-500)"];
        yield return [".btn", "focus:bg-blue-500", ".btn:focus", "background-color: var(--color-blue-500)"];
        yield return [".btn", "active:bg-green-500", ".btn:active", "background-color: var(--color-green-500)"];
        yield return [".btn", "disabled:opacity-50", ".btn:disabled", "opacity: 0.5"];

        // Pseudo-element variants
        yield return [".btn", "before:content-['→']", ".btn::before", "--tw-content: '→'"];
        yield return [".btn", "after:absolute", ".btn::after", "position: absolute"];
        yield return [".btn", "first-line:text-lg", ".btn::first-line", "font-size: 1.125rem"];

        // Functional attribute variants
        yield return [".btn", "data-[active=true]:bg-blue-600", ".btn[data-active=\"true\"]", "background-color: var(--color-blue-600)"];
        yield return [".btn", "aria-[pressed=true]:bg-gray-700", ".btn[aria-pressed=\"true\"]", "background-color: var(--color-gray-700)"];

        // Functional pseudo-class variants
        yield return [".card", "has-[>img]:p-0", ".card:has(>img)", "padding: calc(var(--spacing) * 0)"];
        yield return [".item", "where-[.active]:text-white", ".item:where(.active)", "color: var(--color-white)"];
        yield return [".nav", "is-[.current]:font-bold", ".nav:is(.current)", "font-weight: 700"];
        yield return [".list", "not-[.hidden]:block", ".list:not(.hidden)", "display: block"];

        // Media query variants
        yield return [".btn", "sm:p-4", "@media (min-width: 640px)", "padding: calc(var(--spacing) * 4)"];
        yield return [".btn", "md:text-xl", "@media (min-width: 768px)", "font-size: 1.25rem"];
        yield return [".btn", "lg:w-full", "@media (min-width: 1024px)", "width: 100%"];

        // Dark mode variant
        yield return [".btn", "dark:bg-gray-800", ".btn:where(.dark, .dark *)", "background-color: var(--color-gray-800)"];

        // Combined variants
        yield return [".btn", "hover:focus:bg-purple-600", ".btn:hover:focus", "background-color: var(--color-purple-600)"];
        yield return [".btn", "md:hover:text-2xl", "@media (min-width: 768px)", ".btn:hover"];
        yield return [".btn", "dark:hover:bg-gray-600", ".btn:where(.dark, .dark *):hover", "background-color: var(--color-gray-600)"];
    }

    [Theory]
    [MemberData(nameof(VariantTestData))]
    public void Process_WithVariousVariants_GeneratesCorrectOutput(string selector, string utilities, string expectedSelector, string expectedProperty)
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(selector, utilities)
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Output for debugging
        Console.WriteLine($"Testing: {selector} with {utilities}");
        Console.WriteLine($"Expected selector: {expectedSelector}");
        Console.WriteLine($"Expected property: {expectedProperty}");
        Console.WriteLine("Generated CSS:");
        Console.WriteLine(result);
        Console.WriteLine("---");

        // Assert
        result.ShouldContain("@layer components");
        result.ShouldContain(expectedSelector);
        result.ShouldContain(expectedProperty);
    }

    [Fact]
    public void Debug_HasVariant_InApplies()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".card", "has-[>img]:p-0")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Output for debugging
        Console.WriteLine("Debug Has Variant:");
        Console.WriteLine(result);

        // Basic checks
        result.ShouldContain("@layer components");

        // Check if any component CSS is generated
        var componentStart = result.IndexOf("@layer components", StringComparison.Ordinal);
        if (componentStart > 0)
        {
            var componentEnd = result.IndexOf("@property", componentStart, StringComparison.Ordinal);
            if (componentEnd < 0) componentEnd = result.Length;
            var componentSection = result.Substring(componentStart, componentEnd - componentStart);
            Console.WriteLine("Component section:");
            Console.WriteLine(componentSection);
        }
    }

    [Fact]
    public void Debug_SmVariant_InApplies()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "sm:p-4")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Output for debugging
        Console.WriteLine("Debug sm: Variant:");
        Console.WriteLine(result);

        // Basic checks
        result.ShouldContain("@layer components");

        // Check if any component CSS is generated
        var componentStart = result.IndexOf("@layer components", StringComparison.Ordinal);
        if (componentStart > 0)
        {
            var componentEnd = result.IndexOf("@property", componentStart, StringComparison.Ordinal);
            if (componentEnd < 0) componentEnd = result.Length;
            var componentSection = result.Substring(componentStart, componentEnd - componentStart);
            Console.WriteLine("Component section:");
            Console.WriteLine(componentSection);
        }
    }

    [Fact]
    public void Debug_DarkHoverCombo_InApplies()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "dark:hover:bg-gray-600")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Output for debugging
        Console.WriteLine("Debug dark:hover: Combo:");
        Console.WriteLine(result);

        // Basic checks
        result.ShouldContain("@layer components");

        // Check if any component CSS is generated
        var componentStart = result.IndexOf("@layer components", StringComparison.Ordinal);
        if (componentStart > 0)
        {
            var componentEnd = result.IndexOf("@property", componentStart, StringComparison.Ordinal);
            if (componentEnd < 0) componentEnd = result.Length;
            var componentSection = result.Substring(componentStart, componentEnd - componentStart);
            Console.WriteLine("Component section:");
            Console.WriteLine(componentSection);
        }
    }

    [Fact]
    public void Debug_DarkVariant_InApplies()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "dark:bg-gray-800")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Output for debugging
        Console.WriteLine("Debug dark: Variant:");
        Console.WriteLine(result);

        // Basic checks
        result.ShouldContain("@layer components");

        // Check if any component CSS is generated
        var componentStart = result.IndexOf("@layer components", StringComparison.Ordinal);
        if (componentStart > 0)
        {
            var componentEnd = result.IndexOf("@property", componentStart, StringComparison.Ordinal);
            if (componentEnd < 0) componentEnd = result.Length;
            var componentSection = result.Substring(componentStart, componentEnd - componentStart);
            Console.WriteLine("Component section:");
            Console.WriteLine(componentSection);
        }
    }
}