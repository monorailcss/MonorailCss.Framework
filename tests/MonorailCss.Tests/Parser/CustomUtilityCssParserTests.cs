using System.Collections.Immutable;
using MonorailCss.Parser.Custom;
using Shouldly;

namespace MonorailCss.Tests.Parser;

/// <summary>
/// Tests for the CustomUtilityCssParser that parses @utility directives.
/// </summary>
public class CustomUtilityCssParserTests
{
    private readonly CustomUtilityCssParser _parser;

    public CustomUtilityCssParserTests()
    {
        _parser = new CustomUtilityCssParser();
    }

    [Fact]
    public void ParseCustomUtilities_WithSimpleUtility_ShouldParseCorrectly()
    {
        // Arrange
        const string css = @"
            @utility scrollbar-none {
                scrollbar-width: none;
            }";

        // Act
        var utilities = _parser.ParseCustomUtilities(css).ToList();

        // Assert
        utilities.Count.ShouldBe(1);
        var utility = utilities[0];
        utility.Pattern.ShouldBe("scrollbar-none");
        utility.IsWildcard.ShouldBeFalse();
        utility.Declarations.Count.ShouldBe(1);
        utility.Declarations[0].Property.ShouldBe("scrollbar-width");
        utility.Declarations[0].Value.ShouldBe("none");
        utility.NestedSelectors.Count.ShouldBe(0);
    }

    [Fact]
    public void ParseCustomUtilities_WithMultipleDeclarations_ShouldParseAllDeclarations()
    {
        // Arrange
        const string css = @"
            @utility scrollbar-thin {
                scrollbar-width: thin;
                scrollbar-gutter: stable;
            }";

        // Act
        var utilities = _parser.ParseCustomUtilities(css).ToList();

        // Assert
        utilities.Count.ShouldBe(1);
        var utility = utilities[0];
        utility.Declarations.Count.ShouldBe(2);
        utility.Declarations[0].Property.ShouldBe("scrollbar-width");
        utility.Declarations[0].Value.ShouldBe("thin");
        utility.Declarations[1].Property.ShouldBe("scrollbar-gutter");
        utility.Declarations[1].Value.ShouldBe("stable");
    }

    [Fact]
    public void ParseCustomUtilities_WithNestedSelector_ShouldParseNestedRules()
    {
        // Arrange
        const string css = @"
            @utility scrollbar-none {
                scrollbar-width: none;
                &::-webkit-scrollbar {
                    display: none;
                }
            }";

        // Act
        var utilities = _parser.ParseCustomUtilities(css).ToList();

        // Assert
        utilities.Count.ShouldBe(1);
        var utility = utilities[0];
        utility.Pattern.ShouldBe("scrollbar-none");
        utility.Declarations.Count.ShouldBe(1);
        utility.Declarations[0].Property.ShouldBe("scrollbar-width");
        utility.Declarations[0].Value.ShouldBe("none");

        utility.NestedSelectors.Count.ShouldBe(1);
        utility.NestedSelectors[0].Selector.ShouldBe("&::-webkit-scrollbar");
        utility.NestedSelectors[0].Declarations.Count.ShouldBe(1);
        utility.NestedSelectors[0].Declarations[0].Property.ShouldBe("display");
        utility.NestedSelectors[0].Declarations[0].Value.ShouldBe("none");
    }

    [Fact]
    public void ParseCustomUtilities_WithWildcardPattern_ShouldDetectWildcard()
    {
        // Arrange
        const string css = @"
            @utility scrollbar-thumb-* {
                --tw-scrollbar-thumb-color: --value(--color-*);
            }";

        // Act
        var utilities = _parser.ParseCustomUtilities(css).ToList();

        // Assert
        utilities.Count.ShouldBe(1);
        var utility = utilities[0];
        utility.Pattern.ShouldBe("scrollbar-thumb-*");
        utility.IsWildcard.ShouldBeTrue();
        utility.Declarations.Count.ShouldBe(1);
        utility.Declarations[0].Property.ShouldBe("--tw-scrollbar-thumb-color");
        utility.Declarations[0].Value.ShouldBe("--value(--color-*)");
    }

    [Fact]
    public void ParseCustomUtilities_WithCssVariables_ShouldExtractDependencies()
    {
        // Arrange
        const string css = @"
            @utility scrollbar-color {
                scrollbar-color: var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color);
            }";

        // Act
        var utilities = _parser.ParseCustomUtilities(css).ToList();

        // Assert
        utilities.Count.ShouldBe(1);
        var utility = utilities[0];
        utility.Declarations.Count.ShouldBe(1);
        utility.Declarations[0].Property.ShouldBe("scrollbar-color");
        utility.Declarations[0].Value.ShouldBe("var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color)");

        utility.CustomPropertyDependencies.ShouldContain("--tw-scrollbar-thumb-color");
        utility.CustomPropertyDependencies.ShouldContain("--tw-scrollbar-track-color");
    }

    [Fact]
    public void ParseCustomUtilities_WithMultipleUtilities_ShouldParseAll()
    {
        // Arrange
        const string css = @"
            @utility scrollbar-none {
                scrollbar-width: none;
            }

            @utility scrollbar-thin {
                scrollbar-width: thin;
            }

            @utility scrollbar-auto {
                scrollbar-width: auto;
            }";

        // Act
        var utilities = _parser.ParseCustomUtilities(css).ToList();

        // Assert
        utilities.Count.ShouldBe(3);
        utilities[0].Pattern.ShouldBe("scrollbar-none");
        utilities[1].Pattern.ShouldBe("scrollbar-thin");
        utilities[2].Pattern.ShouldBe("scrollbar-auto");
    }

    [Fact]
    public void ParseCustomUtilities_WithComplexNestedSelectors_ShouldParseCorrectly()
    {
        // Arrange
        const string css = @"
            @utility scrollbar-custom {
                scrollbar-width: thin;
                &::-webkit-scrollbar {
                    width: 8px;
                    height: 8px;
                }
                &::-webkit-scrollbar-thumb {
                    background-color: rgba(0, 0, 0, 0.5);
                    border-radius: 4px;
                }
                &::-webkit-scrollbar-track {
                    background-color: transparent;
                }
            }";

        // Act
        var utilities = _parser.ParseCustomUtilities(css).ToList();

        // Assert
        utilities.Count.ShouldBe(1);
        var utility = utilities[0];
        utility.Pattern.ShouldBe("scrollbar-custom");
        utility.Declarations.Count.ShouldBe(1);
        utility.NestedSelectors.Count.ShouldBe(3);

        utility.NestedSelectors[0].Selector.ShouldBe("&::-webkit-scrollbar");
        utility.NestedSelectors[0].Declarations.Count.ShouldBe(2);

        utility.NestedSelectors[1].Selector.ShouldBe("&::-webkit-scrollbar-thumb");
        utility.NestedSelectors[1].Declarations.Count.ShouldBe(2);

        utility.NestedSelectors[2].Selector.ShouldBe("&::-webkit-scrollbar-track");
        utility.NestedSelectors[2].Declarations.Count.ShouldBe(1);
    }

    [Fact]
    public void ParseCustomUtilities_WithEmptyInput_ShouldReturnEmpty()
    {
        // Arrange
        const string css = "";

        // Act
        var utilities = _parser.ParseCustomUtilities(css).ToList();

        // Assert
        utilities.Count.ShouldBe(0);
    }

    [Fact]
    public void ParseCustomUtilities_WithInvalidSyntax_ShouldSkipInvalid()
    {
        // Arrange
        const string css = @"
            @utility {
                property: value;
            }

            @utility valid-utility {
                property: value;
            }";

        // Act
        var utilities = _parser.ParseCustomUtilities(css).ToList();

        // Assert
        utilities.Count.ShouldBe(1);
        utilities[0].Pattern.ShouldBe("valid-utility");
    }

    [Fact]
    public void ValidateUtilityDefinition_WithValidDefinition_ShouldReturnTrue()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-none",
            Declarations = new List<CssDeclaration>
            {
                new("scrollbar-width", "none")
            }.ToImmutableList()
        };

        // Act
        var isValid = _parser.ValidateUtilityDefinition(definition);

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUtilityDefinition_WithEmptyPattern_ShouldReturnFalse()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "",
            Declarations = new List<CssDeclaration>
            {
                new("property", "value")
            }.ToImmutableList()
        };

        // Act
        var isValid = _parser.ValidateUtilityDefinition(definition);

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void ValidateUtilityDefinition_WithNoDeclarations_ShouldReturnFalse()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "test-utility"
        };

        // Act
        var isValid = _parser.ValidateUtilityDefinition(definition);

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void ValidateUtilityDefinition_WithInvalidPatternCharacters_ShouldReturnFalse()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "Test_Utility!",
            Declarations = new List<CssDeclaration>
            {
                new("property", "value")
            }.ToImmutableList()
        };

        // Act
        var isValid = _parser.ValidateUtilityDefinition(definition);

        // Assert
        isValid.ShouldBeFalse();
    }

    [Fact]
    public void ValidateUtilityDefinition_WithValidWildcard_ShouldReturnTrue()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-thumb-*",
            IsWildcard = true,
            Declarations = new List<CssDeclaration>
            {
                new("--tw-scrollbar-thumb-color", "value")
            }.ToImmutableList()
        };

        // Act
        var isValid = _parser.ValidateUtilityDefinition(definition);

        // Assert
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void ValidateUtilityDefinition_WithInvalidWildcard_ShouldReturnFalse()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-th*umb",
            IsWildcard = true,
            Declarations = new List<CssDeclaration>
            {
                new("property", "value")
            }.ToImmutableList()
        };

        // Act
        var isValid = _parser.ValidateUtilityDefinition(definition);

        // Assert
        isValid.ShouldBeFalse();
    }
}