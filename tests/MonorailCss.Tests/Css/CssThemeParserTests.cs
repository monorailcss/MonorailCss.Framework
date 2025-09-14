using MonorailCss.Css;
using Shouldly;

namespace MonorailCss.Tests.Css;

public class CssThemeParserTests
{
    private readonly CssThemeParser _parser = new();

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyResult()
    {
        // Arrange
        var css = "";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.ThemeVariables.ShouldBeEmpty();
        result.ComponentRules.ShouldBeEmpty();
        result.HasImport.ShouldBeFalse();
    }

    [Fact]
    public void Parse_ImportTailwindcss_SetsHasImport()
    {
        // Arrange
        var css = "@import \"tailwindcss\";";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.HasImport.ShouldBeTrue();
    }

    [Fact]
    public void Parse_ImportTailwindcssWithSingleQuotes_SetsHasImport()
    {
        // Arrange
        var css = "@import 'tailwindcss';";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.HasImport.ShouldBeTrue();
    }

    [Fact]
    public void Parse_ThemeBlock_ExtractsVariables()
    {
        // Arrange
        var css = @"
            @theme {
                --color-orange-500: purple;
                --color-blue-300: #3b82f6;
                --spacing-lg: 2rem;
            }
        ";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.ThemeVariables.Count.ShouldBe(3);
        result.ThemeVariables["--color-orange-500"].ShouldBe("purple");
        result.ThemeVariables["--color-blue-300"].ShouldBe("#3b82f6");
        result.ThemeVariables["--spacing-lg"].ShouldBe("2rem");
    }

    [Fact]
    public void Parse_MultipleThemeBlocks_MergesVariables()
    {
        // Arrange
        var css = @"
            @theme {
                --color-red-500: #ef4444;
            }

            @theme {
                --color-blue-500: #3b82f6;
                --color-red-500: crimson; /* Override */
            }
        ";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.ThemeVariables.Count.ShouldBe(2);
        result.ThemeVariables["--color-red-500"].ShouldBe("crimson"); // Last one wins
        result.ThemeVariables["--color-blue-500"].ShouldBe("#3b82f6");
    }

    [Fact]
    public void Parse_ComponentWithApply_ExtractsUtilities()
    {
        // Arrange
        var css = @"
            .btn {
                @apply bg-red-400 text-white p-4;
            }
        ";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.ComponentRules.Count.ShouldBe(1);
        result.ComponentRules[".btn"].ShouldBe("bg-red-400 text-white p-4");
    }

    [Fact]
    public void Parse_ComponentWithVariants_PreservesVariants()
    {
        // Arrange
        var css = @"
            .btn {
                @apply bg-red-400 dark:bg-green-500 hover:bg-orange-500;
            }
        ";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.ComponentRules.Count.ShouldBe(1);
        result.ComponentRules[".btn"].ShouldBe("bg-red-400 dark:bg-green-500 hover:bg-orange-500");
    }

    [Fact]
    public void Parse_MultipleComponents_ExtractsAll()
    {
        // Arrange
        var css = @"
            .btn {
                @apply bg-blue-500 text-white;
            }

            .card {
                @apply bg-white shadow-lg rounded-lg;
            }
        ";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.ComponentRules.Count.ShouldBe(2);
        result.ComponentRules[".btn"].ShouldBe("bg-blue-500 text-white");
        result.ComponentRules[".card"].ShouldBe("bg-white shadow-lg rounded-lg");
    }

    [Fact]
    public void Parse_ComplexSelector_ExtractsCorrectly()
    {
        // Arrange
        var css = @"
            .btn:hover {
                @apply bg-blue-600;
            }

            #main .card {
                @apply p-6;
            }
        ";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.ComponentRules.Count.ShouldBe(2);
        result.ComponentRules[".btn:hover"].ShouldBe("bg-blue-600");
        result.ComponentRules["#main .card"].ShouldBe("p-6");
    }

    [Fact]
    public void Parse_WithComments_IgnoresComments()
    {
        // Arrange
        var css = @"
            /* This is a comment */
            @theme {
                --color-red-500: #ef4444; /* Red color */
            }

            /* Component styles */
            .btn {
                /* Button utilities */
                @apply bg-blue-500 text-white;
            }
        ";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.ThemeVariables.Count.ShouldBe(1);
        result.ThemeVariables["--color-red-500"].ShouldBe("#ef4444");
        result.ComponentRules.Count.ShouldBe(1);
        result.ComponentRules[".btn"].ShouldBe("bg-blue-500 text-white");
    }

    [Fact]
    public void Parse_FullExample_ParsesEverything()
    {
        // Arrange
        var css = @"
            @import ""tailwindcss"";

            @theme {
                --color-orange-500: purple;
            }

            .btn {
                @apply bg-red-400 dark:bg-green-500 hover:bg-orange-500;
            }
        ";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.HasImport.ShouldBeTrue();
        result.ThemeVariables.Count.ShouldBe(1);
        result.ThemeVariables["--color-orange-500"].ShouldBe("purple");
        result.ComponentRules.Count.ShouldBe(1);
        result.ComponentRules[".btn"].ShouldBe("bg-red-400 dark:bg-green-500 hover:bg-orange-500");
    }

    [Fact]
    public void Parse_MultipleApplyInSameRule_CombinesUtilities()
    {
        // Arrange
        var css = @"
            .btn {
                @apply bg-blue-500;
                @apply text-white p-4;
                @apply hover:bg-blue-600;
            }
        ";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.ComponentRules.Count.ShouldBe(1);
        result.ComponentRules[".btn"].ShouldBe("bg-blue-500 text-white p-4 hover:bg-blue-600");
    }

    [Fact]
    public void Parse_ThemeVariableWithComplexValue_ExtractsCorrectly()
    {
        // Arrange
        var css = @"
            @theme {
                --font-sans: ui-sans-serif, system-ui, sans-serif, 'Apple Color Emoji';
                --shadow-lg: 0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1);
            }
        ";

        // Act
        var result = _parser.Parse(css);

        // Assert
        result.ThemeVariables.Count.ShouldBe(2);
        result.ThemeVariables["--font-sans"].ShouldBe("ui-sans-serif, system-ui, sans-serif, 'Apple Color Emoji'");
        result.ThemeVariables["--shadow-lg"].ShouldBe("0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)");
    }
}