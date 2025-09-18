using MonorailCss.Parser.Custom;
using Shouldly;

namespace MonorailCss.Tests.Parser.Custom;

/// <summary>
/// Integration tests for custom utilities with CssFramework.
/// </summary>
public class CustomUtilityIntegrationTests
{
    private readonly CustomUtilityCssParser _parser = new();
    private readonly CssFramework _framework = new();

    [Fact]
    public void CssFramework_WithSimpleCustomUtility_ShouldGenerateCorrectCss()
    {
        // Arrange
        var css = @"
            @utility scrollbar-thin {
                scrollbar-width: thin;
            }
        ";

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        _framework.AddUtilities(utilities);

        // Act
        var output = _framework.Process("scrollbar-thin");

        // Assert
        output.ShouldContain(".scrollbar-thin");
        output.ShouldContain("scrollbar-width: thin");
    }

    [Fact]
    public void CssFramework_WithNestedSelectorCustomUtility_ShouldGenerateCorrectCss()
    {
        // Arrange
        var css = @"
            @utility scrollbar-none {
                scrollbar-width: none;
                &::-webkit-scrollbar {
                    display: none;
                }
            }
        ";

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        _framework.AddUtilities(utilities);

        // Act
        var output = _framework.Process("scrollbar-none");

        // Assert
        output.ShouldContain(".scrollbar-none");
        output.ShouldContain("scrollbar-width: none");
        output.ShouldContain("::-webkit-scrollbar");
        output.ShouldContain("display: none");
    }

    [Fact]
    public void CssFramework_WithMultipleCustomUtilities_ShouldGenerateCorrectCss()
    {
        // Arrange
        var css = @"
            @utility scrollbar-thin {
                scrollbar-width: thin;
            }

            @utility scrollbar-width-auto {
                scrollbar-width: auto;
            }

            @utility scrollbar-gutter-auto {
                scrollbar-gutter: auto;
            }
        ";

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        _framework.AddUtilities(utilities);

        // Act
        var output1 = _framework.Process("scrollbar-thin");
        var output2 = _framework.Process("scrollbar-width-auto");
        var output3 = _framework.Process("scrollbar-gutter-auto");

        // Assert
        output1.ShouldContain("scrollbar-width: thin");
        output2.ShouldContain("scrollbar-width: auto");
        output3.ShouldContain("scrollbar-gutter: auto");
    }

    [Fact]
    public void CssFramework_WithCssVariableCustomUtility_ShouldGenerateCorrectCss()
    {
        // Arrange
        var css = @"
            @utility scrollbar-both-edges {
                --tw-scrollbar-gutter-modifier: both-edges;
                scrollbar-gutter: stable var(--tw-scrollbar-gutter-modifier);
            }
        ";

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        _framework.AddUtilities(utilities);

        // Act
        var output = _framework.Process("scrollbar-both-edges");

        // Assert
        output.ShouldContain(".scrollbar-both-edges");
        output.ShouldContain("--tw-scrollbar-gutter-modifier: both-edges");
        output.ShouldContain("scrollbar-gutter: stable var(--tw-scrollbar-gutter-modifier)");
    }

    [Fact]
    public void CssFramework_CustomUtilityWithVariants_ShouldGenerateCorrectCss()
    {
        // Arrange
        var css = @"
            @utility custom-test {
                color: red;
            }
        ";

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        _framework.AddUtilities(utilities);

        // Act
        var output = _framework.Process("hover:custom-test");

        // Assert
        output.ShouldContain("color: red");
        output.ShouldContain("hover"); // Should include hover variant
    }

    [Fact]
    public void CssFramework_CustomUtilityWithImportant_ShouldGenerateImportantCss()
    {
        // Arrange
        var css = @"
            @utility custom-important {
                display: block;
            }
        ";

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        _framework.AddUtilities(utilities);

        // Act
        var output = _framework.Process("!custom-important");

        // Assert
        output.ShouldContain("display: block !important");
    }

    [Fact]
    public void CssFramework_ComplexScrollbarUtility_ShouldGenerateCorrectCss()
    {
        // Arrange
        var css = @"
            @utility scrollbar-stable {
                scrollbar-gutter: stable;
            }

            @utility scrollbar-both-edges {
                --tw-scrollbar-gutter-modifier: both-edges;
                scrollbar-gutter: stable var(--tw-scrollbar-gutter-modifier);
            }

            @utility scrollbar-color {
                scrollbar-color: var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color);
            }
        ";

        var definitions = _parser.ParseCustomUtilities(css);
        var utilities = CustomUtilityFactory.CreateUtilities(definitions);
        _framework.AddUtilities(utilities);

        // Act & Assert
        var output1 = _framework.Process("scrollbar-stable");
        output1.ShouldContain("scrollbar-gutter: stable");

        var output2 = _framework.Process("scrollbar-both-edges");
        output2.ShouldContain("--tw-scrollbar-gutter-modifier: both-edges");
        output2.ShouldContain("scrollbar-gutter: stable var(--tw-scrollbar-gutter-modifier)");

        var output3 = _framework.Process("scrollbar-color");
        output3.ShouldContain("scrollbar-color: var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color)");
    }
}