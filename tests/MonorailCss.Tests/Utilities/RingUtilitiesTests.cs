using System.Collections.Immutable;
using Shouldly;

namespace MonorailCss.Tests.Utilities;

public class RingUtilitiesTests
{
    private readonly CssFramework _cssFramework = new(new CssFrameworkSettings
    {
        IncludePreflight = false
    });

    [Theory]
    [InlineData("ring", "box-shadow:")]
    [InlineData("ring-0", "box-shadow:")]
    [InlineData("ring-1", "box-shadow:")]
    [InlineData("ring-2", "box-shadow:")]
    [InlineData("ring-4", "box-shadow:")]
    [InlineData("ring-8", "box-shadow:")]
    public void RingWidth_ShouldGenerateBoxShadow(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);
        Console.WriteLine(result);
        // Assert
        result.ShouldContain(expectedProperty);
        result.ShouldContain("--tw-ring-shadow");
    }

    [Fact]
    public void Ring_Default_ShouldUse3pxWidth()
    {
        // Act
        var result = _cssFramework.Process("ring");

        // Assert
        result.ShouldContain("calc(3px + var(--tw-ring-offset-width))");
    }

    [Theory]
    [InlineData("ring-[10px]", "calc(10px + var(--tw-ring-offset-width))")]
    [InlineData("ring-[0.5rem]", "calc(0.5rem + var(--tw-ring-offset-width))")]
    public void RingWidth_WithArbitraryValue_ShouldWork(string className, string expectedValue)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedValue);
    }

    [Theory]
    [InlineData("ring-red-500", "--tw-ring-color:")]
    [InlineData("ring-blue-600", "--tw-ring-color:")]
    [InlineData("ring-transparent", "--tw-ring-color: transparent")]
    [InlineData("ring-current", "--tw-ring-color: currentColor")]
    public void RingColor_ShouldSetRingColorVariable(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
    }

    [Theory]
    [InlineData("ring-red-500/50", "color-mix(in oklab,")]
    [InlineData("ring-blue-600/25", "color-mix(in oklab,")]
    public void RingColor_WithOpacity_ShouldUseColorMix(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
        result.ShouldContain("--tw-ring-color:");
    }

    [Theory]
    [InlineData("ring-[#ff0000]", "--tw-ring-color: #ff0000")]
    [InlineData("ring-[rgb(255,0,0)]", "--tw-ring-color: rgb(255,0,0)")]
    public void RingColor_WithArbitraryValue_ShouldWork(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
    }

    [Theory]
    [InlineData("ring-offset-0", "--tw-ring-offset-width: 0px")]
    [InlineData("ring-offset-1", "--tw-ring-offset-width:")]
    [InlineData("ring-offset-2", "--tw-ring-offset-width:")]
    [InlineData("ring-offset-4", "--tw-ring-offset-width:")]
    [InlineData("ring-offset-8", "--tw-ring-offset-width:")]
    public void RingOffsetWidth_ShouldSetOffsetWidthVariable(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
    }

    [Theory]
    [InlineData("ring-offset-[5px]", "--tw-ring-offset-width: 5px")]
    [InlineData("ring-offset-[0.25rem]", "--tw-ring-offset-width: 0.25rem")]
    public void RingOffsetWidth_WithArbitraryValue_ShouldWork(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
    }

    [Theory]
    [InlineData("ring-offset-red-500", "--tw-ring-offset-color:")]
    [InlineData("ring-offset-blue-600", "--tw-ring-offset-color:")]
    [InlineData("ring-offset-transparent", "--tw-ring-offset-color: transparent")]
    [InlineData("ring-offset-current", "--tw-ring-offset-color: currentColor")]
    public void RingOffsetColor_ShouldSetOffsetColorVariable(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
    }

    [Theory]
    [InlineData("ring-offset-red-500/50", "color-mix(in oklab,")]
    [InlineData("ring-offset-blue-600/25", "color-mix(in oklab,")]
    public void RingOffsetColor_WithOpacity_ShouldUseColorMix(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
        result.ShouldContain("--tw-ring-offset-color:");
    }

    [Fact]
    public void RingUtilities_ShouldStackCorrectly()
    {
        // Test that multiple ring utilities can be combined
        var result = _cssFramework.Process("ring-2 ring-blue-500 ring-offset-2 ring-offset-white");

        // All properties should be present
        result.ShouldContain("box-shadow:");
        result.ShouldContain("--tw-ring-shadow");
        result.ShouldContain("--tw-ring-color:");
        result.ShouldContain("--tw-ring-offset-width:");
        result.ShouldContain("--tw-ring-offset-color:");
    }

    [Fact]
    public void Ring_WithCustomColorPalette_ShouldWork()
    {
        // Arrange
        var theme = new MonorailCss.Theme.Theme()
            .AddColorPalette("brand", ImmutableDictionary.Create<string, string>()
                .Add("500", "oklch(71.2% 0.191 280)"));
        var settings = new CssFrameworkSettings
        {
            Theme = theme
        };
        var cssFramework = new CssFramework(settings);

        // Act
        var result = cssFramework.Process("ring-brand-500");

        // Assert
        result.ShouldContain("--tw-ring-color:");
        result.ShouldContain("var(--color-brand-500)");
    }
}