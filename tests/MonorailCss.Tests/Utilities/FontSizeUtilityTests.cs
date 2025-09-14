using Shouldly;

namespace MonorailCss.Tests.Utilities;

public class FontSizeUtilityTests
{
    private readonly CssFramework _cssFramework = new();

    [Theory]
    [InlineData("text-xs", "font-size: 0.75rem")]
    [InlineData("text-sm", "font-size: 0.875rem")]
    [InlineData("text-base", "font-size: 1rem")]
    [InlineData("text-lg", "font-size: 1.125rem")]
    [InlineData("text-xl", "font-size: 1.25rem")]
    [InlineData("text-2xl", "font-size: 1.5rem")]
    [InlineData("text-3xl", "font-size: 1.875rem")]
    [InlineData("text-4xl", "font-size: 2.25rem")]
    [InlineData("text-5xl", "font-size: 3rem")]
    [InlineData("text-6xl", "font-size: 3.75rem")]
    [InlineData("text-7xl", "font-size: 4.5rem")]
    [InlineData("text-8xl", "font-size: 6rem")]
    [InlineData("text-9xl", "font-size: 8rem")]
    public void FontSize_ShouldGenerateCorrectFontSize(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
    }

    [Theory]
    [InlineData("text-xs", "line-height:")]
    [InlineData("text-sm", "line-height:")]
    [InlineData("text-base", "line-height:")]
    [InlineData("text-lg", "line-height:")]
    [InlineData("text-xl", "line-height:")]
    [InlineData("text-2xl", "line-height:")]
    [InlineData("text-3xl", "line-height:")]
    [InlineData("text-4xl", "line-height:")]
    [InlineData("text-5xl", "line-height:")]
    [InlineData("text-6xl", "line-height:")]
    [InlineData("text-7xl", "line-height:")]
    [InlineData("text-8xl", "line-height:")]
    [InlineData("text-9xl", "line-height:")]
    public void FontSize_ShouldAlsoSetLineHeight(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
        result.ShouldContain("font-size:");
    }

    [Theory]
    [InlineData("text-xs", "var(--text-xs--line-height)")]
    [InlineData("text-sm", "var(--text-sm--line-height)")]
    [InlineData("text-base", "var(--text-base--line-height)")]
    [InlineData("text-lg", "var(--text-lg--line-height)")]
    [InlineData("text-xl", "var(--text-xl--line-height)")]
    public void FontSize_ShouldUseThemeLineHeightVariables(string className, string expectedVariable)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedVariable);
    }

    [Theory]
    [InlineData("text-[14px]", "font-size: 14px")]
    [InlineData("text-[1.5rem]", "font-size: 1.5rem")]
    [InlineData("text-[2em]", "font-size: 2em")]
    public void FontSize_WithArbitraryValue_ShouldWork(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
    }

    [Fact]
    public void FontSize_WithArbitraryValue_ShouldNotIncludeLineHeight()
    {
        // Act
        var result = _cssFramework.Process("text-[14px]");

        // Assert
        result.ShouldContain("font-size: 14px");
        // Check that the specific utility class doesn't generate line-height
        // (base styles may contain line-height, but the arbitrary font-size utility shouldn't)
        var classPattern = @".text-\[14px\] {";
        var classIndex = result.IndexOf(classPattern, StringComparison.Ordinal);
        if (classIndex >= 0)
        {
            var closingBraceIndex = result.IndexOf('}', classIndex);
            var classContent = result.Substring(classIndex, closingBraceIndex - classIndex + 1);
            classContent.ShouldNotContain("line-height:");
        }
    }

    [Fact]
    public void FontSize_WithCustomFontFamily_ShouldNotInterfere()
    {
        // Arrange
        var theme = new MonorailCss.Theme.Theme()
            .AddFontFamily("display", "'Playfair Display', serif");
        var settings = new CssFrameworkSettings { Theme = theme };
        var cssFramework = new CssFramework(settings);

        // Act
        var fontSizeResult = cssFramework.Process("text-lg");
        var fontFamilyResult = cssFramework.Process("font-display");

        // Assert
        fontSizeResult.ShouldContain("font-size:");
        fontSizeResult.ShouldContain("line-height:");
        fontFamilyResult.ShouldContain("font-family:");
        fontFamilyResult.ShouldContain("var(--font-display)");
    }

    [Theory]
    [InlineData("text-red-500", "color:")] // Should be text color, not font size
    [InlineData("text-blue-600", "color:")] // Should be text color, not font size
    public void FontSize_ShouldNotMatchTextColorUtilities(string className, string expectedProperty)
    {
        // Act
        var result = _cssFramework.Process(className);

        // Assert
        result.ShouldContain(expectedProperty);
        // Check that the specific utility class doesn't generate font-size
        // (base styles may contain font-size, but the utility class shouldn't)
        var classPattern = $".{className.Replace("[", "\\[").Replace("]", "\\]")} {{";
        var classIndex = result.IndexOf(classPattern, StringComparison.Ordinal);
        if (classIndex >= 0)
        {
            var closingBraceIndex = result.IndexOf('}', classIndex);
            var classContent = result.Substring(classIndex, closingBraceIndex - classIndex + 1);
            classContent.ShouldNotContain("font-size:");
        }
    }

    [Fact]
    public void FontSize_WithImportant_ShouldAddImportant()
    {
        // Act
        var result = _cssFramework.Process("text-lg!");

        // Assert
        result.ShouldContain("font-size:");
        result.ShouldContain("line-height:");
        result.ShouldContain("!important");
    }

    [Fact]
    public void FontSize_MultipleSizes_ShouldAllWork()
    {
        // Test that we can use multiple font sizes in one process call
        var result = _cssFramework.Process("text-xs text-sm text-base text-lg text-xl");

        // All font sizes should be present
        result.ShouldContain("font-size: 0.75rem");
        result.ShouldContain("font-size: 0.875rem");
        result.ShouldContain("font-size: 1rem");
        result.ShouldContain("font-size: 1.125rem");
        result.ShouldContain("font-size: 1.25rem");
    }
}