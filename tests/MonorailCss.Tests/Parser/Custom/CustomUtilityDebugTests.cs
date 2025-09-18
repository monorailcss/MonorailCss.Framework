using MonorailCss.Parser.Custom;
using Shouldly;

namespace MonorailCss.Tests.Parser.Custom;

/// <summary>
/// Debug tests to understand custom utility behavior.
/// </summary>
public class CustomUtilityDebugTests
{
    [Fact]
    public void Debug_ParseAndCreateUtility()
    {
        // Arrange
        var parser = new CustomUtilityCssParser();
        var css = @"
            @utility scrollbar-thin {
                scrollbar-width: thin;
            }
        ";

        // Act - Parse CSS
        var definitions = parser.ParseCustomUtilities(css).ToList();

        // Assert - Check parsing worked
        definitions.Count.ShouldBe(1);
        var definition = definitions[0];
        definition.Pattern.ShouldBe("scrollbar-thin");
        definition.Declarations.Count.ShouldBe(1);
        definition.Declarations[0].Property.ShouldBe("scrollbar-width");
        definition.Declarations[0].Value.ShouldBe("thin");

        // Act - Create utility
        var utility = CustomUtilityFactory.CreateStaticUtility(definition);
        utility.ShouldNotBeNull();

        // Act - Try to compile with a candidate
        var candidate = new MonorailCss.Candidates.StaticUtility
        {
            Raw = "scrollbar-thin",
            Root = "scrollbar-thin",
            Variants = [],
            Important = false
        };

        var theme = new MonorailCss.Theme.Theme();
        var compiled = utility.TryCompile(candidate, theme, out var nodes);

        // Assert - Check compilation
        compiled.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0] as MonorailCss.Ast.Declaration;
        declaration.ShouldNotBeNull();
        declaration.Property.ShouldBe("scrollbar-width");
        declaration.Value.ShouldBe("thin");
    }

    [Fact]
    public void Debug_UtilityInFramework()
    {
        // Arrange
        var parser = new CustomUtilityCssParser();
        var framework = new CssFramework();

        var css = @"
            @utility scrollbar-thin {
                scrollbar-width: thin;
            }
        ";

        // Parse and create utility
        var definitions = parser.ParseCustomUtilities(css).ToList();
        var utility = CustomUtilityFactory.CreateStaticUtility(definitions[0]);

        // Act - Add to framework
        framework.AddUtility(utility);

        // Try to process
        var output = framework.Process("scrollbar-thin");

        // Debug output
        Console.WriteLine("CSS Output:");
        Console.WriteLine(output);

        // Check if it contains our CSS
        var containsScrollbar = output.Contains("scrollbar-width");
        containsScrollbar.ShouldBeTrue($"Output should contain 'scrollbar-width' but was: {output.Substring(0, Math.Min(500, output.Length))}");
    }
}