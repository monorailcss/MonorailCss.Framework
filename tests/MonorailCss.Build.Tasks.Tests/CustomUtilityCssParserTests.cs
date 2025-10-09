using MonorailCss.Build.Tasks.Parsing;
using Shouldly;

namespace MonorailCss.Build.Tasks.Tests;

public class CustomUtilityCssParserTests
{
    private readonly CustomUtilityCssParser _parser = new();

    [Fact]
    public void ParseCustomUtilities_WithBasicApply_ExtractsUtilities()
    {
        // Arrange
        const string css = """
            @utility bordered-link {
                @apply font-semibold leading-tight;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        result.ShouldNotBeEmpty();
        var utility = result[0];
        utility.Pattern.ShouldBe("bordered-link");
        utility.IsWildcard.ShouldBeFalse();
        utility.ApplyUtilities.Count.ShouldBe(2);
        utility.ApplyUtilities[0].ShouldBe("font-semibold");
        utility.ApplyUtilities[1].ShouldBe("leading-tight");
    }

    [Fact]
    public void ParseCustomUtilities_WithMultipleUtilitiesInApply_SplitsCorrectly()
    {
        // Arrange
        const string css = """
            @utility bordered-link {
                @apply font-semibold leading-tight text-current border-b border-current hover:border-b-2;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.ApplyUtilities.Count.ShouldBe(6);
        utility.ApplyUtilities[0].ShouldBe("font-semibold");
        utility.ApplyUtilities[1].ShouldBe("leading-tight");
        utility.ApplyUtilities[2].ShouldBe("text-current");
        utility.ApplyUtilities[3].ShouldBe("border-b");
        utility.ApplyUtilities[4].ShouldBe("border-current");
        utility.ApplyUtilities[5].ShouldBe("hover:border-b-2");
    }

    [Fact]
    public void ParseCustomUtilities_WithVariantPrefixes_PreservesColons()
    {
        // Arrange
        const string css = """
            @utility interactive {
                @apply hover:underline focus:ring-2 active:scale-95;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.ApplyUtilities.Count.ShouldBe(3);
        utility.ApplyUtilities[0].ShouldBe("hover:underline");
        utility.ApplyUtilities[1].ShouldBe("focus:ring-2");
        utility.ApplyUtilities[2].ShouldBe("active:scale-95");
    }

    [Fact]
    public void ParseCustomUtilities_WithMultipleApplyDirectives_CombinesUtilities()
    {
        // Arrange
        const string css = """
            @utility card {
                @apply bg-white rounded-lg;
                @apply shadow-md p-4;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.ApplyUtilities.Count.ShouldBe(4);
        utility.ApplyUtilities[0].ShouldBe("bg-white");
        utility.ApplyUtilities[1].ShouldBe("rounded-lg");
        utility.ApplyUtilities[2].ShouldBe("shadow-md");
        utility.ApplyUtilities[3].ShouldBe("p-4");
    }

    [Fact]
    public void ParseCustomUtilities_WithApplyAndDeclarations_ParsesBoth()
    {
        // Arrange
        const string css = """
            @utility custom-button {
                @apply font-bold px-4 py-2;
                background: linear-gradient(to right, red, blue);
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.ApplyUtilities.Count.ShouldBe(3);
        utility.ApplyUtilities[0].ShouldBe("font-bold");
        utility.ApplyUtilities[1].ShouldBe("px-4");
        utility.ApplyUtilities[2].ShouldBe("py-2");

        utility.Declarations.Count.ShouldBe(1);
        utility.Declarations[0].Property.ShouldBe("background");
        utility.Declarations[0].Value.ShouldBe("linear-gradient(to right, red, blue)");
    }

    [Fact]
    public void ParseCustomUtilities_WithApplyAndNestedSelectors_ParsesBoth()
    {
        // Arrange
        const string css = """
            @utility scrollbar-custom {
                @apply w-2 h-2;
                &::-webkit-scrollbar {
                    width: 8px;
                }
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.ApplyUtilities.Count.ShouldBe(2);
        utility.ApplyUtilities[0].ShouldBe("w-2");
        utility.ApplyUtilities[1].ShouldBe("h-2");

        utility.NestedSelectors.Count.ShouldBe(1);
        utility.NestedSelectors[0].Selector.ShouldBe("&::-webkit-scrollbar");
        utility.NestedSelectors[0].Declarations.Count.ShouldBe(1);
    }

    [Fact]
    public void ParseCustomUtilities_WildcardWithApply_ParsesCorrectly()
    {
        // Arrange
        const string css = """
            @utility highlight-* {
                @apply border-2;
                box-shadow: inset 0 1px 0 0 red;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.Pattern.ShouldBe("highlight-*");
        utility.IsWildcard.ShouldBeTrue();
        utility.ApplyUtilities.Count.ShouldBe(1);
        utility.ApplyUtilities[0].ShouldBe("border-2");
        utility.Declarations.Count.ShouldBe(1);
    }

    [Fact]
    public void ParseCustomUtilities_WithEmptyApply_ReturnsEmptyList()
    {
        // Arrange
        const string css = """
            @utility empty {
                @apply ;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.ApplyUtilities.ShouldBeEmpty();
    }

    [Fact]
    public void ParseCustomUtilities_WithoutApply_HasEmptyApplyList()
    {
        // Arrange
        const string css = """
            @utility no-apply {
                background: red;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.ApplyUtilities.ShouldBeEmpty();
        utility.Declarations.Count.ShouldBe(1);
    }

    [Fact]
    public void ParseCustomUtilities_WithCommentsInApply_IgnoresComments()
    {
        // Arrange
        const string css = """
            @utility commented {
                /* This is a comment */
                @apply font-bold /* inline comment */ text-center;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.ApplyUtilities.Count.ShouldBe(2);
        utility.ApplyUtilities[0].ShouldBe("font-bold");
        utility.ApplyUtilities[1].ShouldBe("text-center");
    }

    [Fact]
    public void ParseCustomUtilities_WithMultilineApply_ParsesCorrectly()
    {
        // Arrange
        const string css = """
            @utility multiline {
                @apply
                    font-bold
                    text-center
                    underline;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.ApplyUtilities.Count.ShouldBe(3);
        utility.ApplyUtilities[0].ShouldBe("font-bold");
        utility.ApplyUtilities[1].ShouldBe("text-center");
        utility.ApplyUtilities[2].ShouldBe("underline");
    }

    [Fact]
    public void ParseCustomUtilities_RealWorldExample_BorderedLink()
    {
        // Arrange - Your exact example from the CSS
        const string css = """
            @utility bordered-link {
                @apply font-semibold leading-tight text-current border-b border-current hover:border-b-2;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.Pattern.ShouldBe("bordered-link");
        utility.IsWildcard.ShouldBeFalse();
        utility.ApplyUtilities.Count.ShouldBe(6);
        utility.ApplyUtilities.ShouldContain("font-semibold");
        utility.ApplyUtilities.ShouldContain("leading-tight");
        utility.ApplyUtilities.ShouldContain("text-current");
        utility.ApplyUtilities.ShouldContain("border-b");
        utility.ApplyUtilities.ShouldContain("border-current");
        utility.ApplyUtilities.ShouldContain("hover:border-b-2");
    }

    [Fact]
    public void ParseCustomUtilities_RealWorldExample_HighlightWithApply()
    {
        // Arrange - Modified version of your highlight-* example with @apply
        const string css = """
            @utility highlight-* {
                @apply border-opacity-50;
                box-shadow: inset 0 1px 0 0 red;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.Pattern.ShouldBe("highlight-*");
        utility.IsWildcard.ShouldBeTrue();
        utility.ApplyUtilities.Count.ShouldBe(1);
        utility.ApplyUtilities[0].ShouldBe("border-opacity-50");
        utility.Declarations.Count.ShouldBe(1);
        utility.Declarations[0].Property.ShouldBe("box-shadow");
    }

    [Fact]
    public void ParseCustomUtilities_WithStaticUtility_ParsesCorrectly()
    {
        // Arrange
        const string css = """
            @utility scrollbar-none {
                scrollbar-width: none;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        result.ShouldNotBeEmpty();
        var utility = result[0];
        utility.Pattern.ShouldBe("scrollbar-none");
        utility.IsWildcard.ShouldBeFalse();
        utility.Declarations.Count.ShouldBe(1);
        utility.Declarations[0].Property.ShouldBe("scrollbar-width");
        utility.Declarations[0].Value.ShouldBe("none");
        utility.ApplyUtilities.ShouldBeEmpty();
    }

    [Fact]
    public void ParseCustomUtilities_WithWildcardPattern_SetsIsWildcard()
    {
        // Arrange
        const string css = """
            @utility scrollbar-thumb-* {
                background-color: red;
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.Pattern.ShouldBe("scrollbar-thumb-*");
        utility.IsWildcard.ShouldBeTrue();
    }

    [Fact]
    public void ParseCustomUtilities_WithNestedSelector_ParsesCorrectly()
    {
        // Arrange
        const string css = """
            @utility custom-scrollbar {
                &::-webkit-scrollbar {
                    width: 8px;
                }
            }
            """;

        // Act
        var result = _parser.ParseCustomUtilities(css);

        // Assert
        var utility = result[0];
        utility.NestedSelectors.Count.ShouldBe(1);
        utility.NestedSelectors[0].Selector.ShouldBe("&::-webkit-scrollbar");
        utility.NestedSelectors[0].Declarations.Count.ShouldBe(1);
    }
}
