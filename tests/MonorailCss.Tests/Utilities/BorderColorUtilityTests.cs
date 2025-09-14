using System.Collections.Immutable;
using Shouldly;

namespace MonorailCss.Tests.Utilities;

public class BorderColorUtilityTests
{
    private readonly CssFramework _cssFramework = new(new CssFrameworkSettings
    {
        IncludePreflight = false
    });

    [Theory]
    [InlineData("border-red-500", "border-color:")]
    [InlineData("border-blue-600", "border-color:")]
    [InlineData("border-green-400", "border-color:")]
    [InlineData("border-transparent", "border-color: transparent")]
    [InlineData("border-current", "border-color: currentColor")]
    public void BorderColor_ShouldGenerateCorrectCss(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
    }

    [Theory]
    [InlineData("border-red-500/50", "color-mix(in oklab,")]
    [InlineData("border-blue-600/25", "color-mix(in oklab,")]
    [InlineData("border-green-400/75", "color-mix(in oklab,")]
    public void BorderColor_WithOpacity_ShouldUseColorMix(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
        result.ShouldContain("border-color:");
    }

    [Theory]
    [InlineData("border-[#123456]", "border-color: #123456")]
    [InlineData("border-[rgb(255,0,0)]", "border-color: rgb(255,0,0)")]
    [InlineData("border-[hsl(120,100%,50%)]", "border-color: hsl(120,100%,50%)")]
    public void BorderColor_WithArbitraryValue_ShouldWork(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        Console.WriteLine(result);

        result.ShouldContain(expectedProperty);
    }

    [Fact]
    public void BorderColor_WithCustomPalette_ShouldWork()
    {
        // Arrange
        var theme = new MonorailCss.Theme.Theme()
            .AddColorPalette("brand", ImmutableDictionary.Create<string, string>()
                .Add("500", "oklch(71.2% 0.191 280)"));
        var settings = new CssFrameworkSettings { Theme = theme };
        var cssFramework = new CssFramework(settings);

        // Act
        var result = cssFramework.Process("border-brand-500");

        // Assert
        result.ShouldContain("border-color:");
        result.ShouldContain("var(--color-brand-500)");
    }

    [Theory]
    [InlineData("border-2", "border-width:")] // Should not match - this is border-width
    [InlineData("border-solid", "border-style:")] // Should not match - this is border-style
    [InlineData("border", "border-width:")] // Should not match - this is border shorthand
    public void BorderColor_ShouldNotMatchOtherBorderUtilities(string className, string notExpectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldNotContain("border-color:");
        result.ShouldContain(notExpectedProperty);
    }

    [Fact]
    public void BorderColor_WithImportant_ShouldAddImportant()
    {
        // Act
        var result = _cssFramework.Process("border-red-500!");

        // Assert
        result.ShouldContain("border-color:");
        result.ShouldContain("!important");
    }
}