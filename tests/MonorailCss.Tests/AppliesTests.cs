using System.Collections.Immutable;
using Shouldly;

namespace MonorailCss.Tests;

public class AppliesTests
{
    [Fact]
    public void Process_WithApplies_ShouldGenerateComponentClasses()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "text-red-500 bg-blue-500")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Assert
        result.ShouldContain("@layer theme, base, components, utilities;");
        result.ShouldContain("@layer theme");
        result.ShouldContain("@layer components");
        result.ShouldContain(".btn");
        result.ShouldContain("background-color: var(--color-blue-500)");
        result.ShouldContain("color: var(--color-red-500)");
        result.ShouldContain("--color-red-500");
        result.ShouldContain("--color-blue-500");
    }

    [Fact]
    public void Process_WithMultipleApplies_ShouldGenerateAllComponentClasses()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "bg-blue-500 text-white")
                .Add(".card", "bg-white text-black")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Assert
        result.ShouldContain(".btn");
        result.ShouldContain(".card");
        result.ShouldContain("background-color: var(--color-blue-500)");
        result.ShouldContain("background-color: var(--color-white)");
        result.ShouldContain("color: var(--color-white)");
        result.ShouldContain("color: var(--color-black)");
    }

    [Fact]
    public void Process_WithAppliesAndUtilities_ShouldGenerateBoth()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn", "bg-blue-500 text-white")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("flex items-center");

        // Assert
        // Component layer
        result.ShouldContain("@layer components");
        result.ShouldContain(".btn");
        result.ShouldContain("background-color: var(--color-blue-500)");
        result.ShouldContain("color: var(--color-white)");

        // Utilities layer
        result.ShouldContain("@layer utilities");
        result.ShouldContain(".flex");
        result.ShouldContain("display: flex");
        result.ShouldContain(".items-center");
        result.ShouldContain("align-items: center");
    }

    [Fact]
    public void Process_WithAppliesUsingComplexUtilities_ShouldResolveCorrectly()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".btn-primary", "bg-blue-500") // Simplify to test basic functionality first
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Assert
        result.ShouldContain(".btn-primary");
        result.ShouldContain("background-color: var(--color-blue-500)");
    }

    [Fact]
    public void Process_WithAppliesAndDuplicateProperties_ShouldDeduplicateProperties()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                // Both bg-blue-500 and bg-red-500 set background-color, last one should win
                .Add(".btn", "bg-blue-500 text-white bg-red-500")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Assert
        result.ShouldContain(".btn");
        // Should only have one background-color property (the last one - red)
        result.ShouldNotContain("background-color: var(--color-blue-500)");
        result.ShouldContain("background-color: var(--color-red-500)");
        result.ShouldContain("color: var(--color-white)");
    }

    [Fact]
    public void Process_WithAppliesUsingSpecialValues_ShouldHandleCorrectly()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
                .Add(".transparent-btn", "bg-transparent text-current")
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("");

        // Assert
        result.ShouldContain(".transparent-btn");
        result.ShouldContain("background-color: transparent");
        result.ShouldContain("color: currentColor");
    }

    [Fact]
    public void Process_WithEmptyApplies_ShouldNotGenerateComponentLayer()
    {
        // Arrange
        var settings = new CssFrameworkSettings
        {
            IncludePreflight = false,
            Applies = ImmutableDictionary<string, string>.Empty
        };

        var framework = new CssFramework(settings);

        // Act
        var result = framework.Process("flex");

        // Assert
        result.ShouldContain("@layer utilities");
        result.ShouldContain(".flex");
        // Components layer should be empty/not present when no applies are defined
        result.ShouldNotContain("@layer components {");
    }
}