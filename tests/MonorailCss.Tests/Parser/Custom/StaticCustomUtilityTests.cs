using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Parser.Custom;
using Shouldly;

namespace MonorailCss.Tests.Parser.Custom;

/// <summary>
/// Tests for StaticCustomUtility implementation.
/// </summary>
public class StaticCustomUtilityTests
{
    private readonly MonorailCss.Theme.Theme _theme = new();

    [Fact]
    public void StaticCustomUtility_SimpleDeclaration_ShouldGenerateCorrectAst()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-thin",
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-width", "thin")
            )
        };

        var utility = new StaticCustomUtility(definition);
        var candidate = new StaticUtility { Raw = "scrollbar-thin", Root = "scrollbar-thin", Variants = [], Important = false };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("scrollbar-width");
        declaration.Value.ShouldBe("thin");
        declaration.Important.ShouldBeFalse();
    }

    [Fact]
    public void StaticCustomUtility_MultipleDeclarations_ShouldGenerateMultipleNodes()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "custom-test",
            Declarations = ImmutableList.Create(
                new CssDeclaration("property-one", "value-one"),
                new CssDeclaration("property-two", "value-two"),
                new CssDeclaration("property-three", "value-three")
            )
        };

        var utility = new StaticCustomUtility(definition);
        var candidate = new StaticUtility { Raw = "custom-test", Root = "custom-test", Variants = [], Important = false };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(3);

        nodes[0].ShouldBeOfType<Declaration>().Property.ShouldBe("property-one");
        nodes[1].ShouldBeOfType<Declaration>().Property.ShouldBe("property-two");
        nodes[2].ShouldBeOfType<Declaration>().Property.ShouldBe("property-three");
    }

    [Fact]
    public void StaticCustomUtility_WithNestedSelector_ShouldGenerateNestedRule()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-none",
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-width", "none")
            ),
            NestedSelectors = ImmutableList.Create(
                new NestedSelector(
                    "&::-webkit-scrollbar",
                    ImmutableList.Create(new CssDeclaration("display", "none"))
                )
            )
        };

        var utility = new StaticCustomUtility(definition);
        var candidate = new StaticUtility { Raw = "scrollbar-none", Root = "scrollbar-none", Variants = [], Important = false };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(2);

        // Check root declaration
        nodes[0].ShouldBeOfType<Declaration>().Property.ShouldBe("scrollbar-width");

        // Check nested rule
        var nestedRule = nodes[1].ShouldBeOfType<NestedRule>();
        nestedRule.Selector.ShouldBe("::-webkit-scrollbar");
        nestedRule.Nodes.Count.ShouldBe(1);
        nestedRule.Nodes[0].ShouldBeOfType<Declaration>().Property.ShouldBe("display");
    }

    [Fact]
    public void StaticCustomUtility_WithImportantFlag_ShouldApplyToAllDeclarations()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "custom-important",
            Declarations = ImmutableList.Create(
                new CssDeclaration("property", "value")
            ),
            NestedSelectors = ImmutableList.Create(
                new NestedSelector(
                    "&:hover",
                    ImmutableList.Create(new CssDeclaration("nested-property", "nested-value"))
                )
            )
        };

        var utility = new StaticCustomUtility(definition);
        var candidate = new StaticUtility { Raw = "custom-important", Root = "custom-important", Variants = [], Important = true }; // Important flag set

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();

        // Check root declaration has important
        nodes[0].ShouldBeOfType<Declaration>().Important.ShouldBeTrue();

        // Check nested declaration has important
        var nestedRule = nodes[1].ShouldBeOfType<NestedRule>();
        nestedRule.Nodes[0].ShouldBeOfType<Declaration>().Important.ShouldBeTrue();
    }

    [Fact]
    public void StaticCustomUtility_WithCssVariables_ShouldGenerateCorrectDeclarations()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-both-edges",
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-gutter-modifier", "both-edges"),
                new CssDeclaration("scrollbar-gutter", "stable var(--tw-scrollbar-gutter-modifier)")
            )
        };

        var utility = new StaticCustomUtility(definition);
        var candidate = new StaticUtility { Raw = "scrollbar-both-edges", Root = "scrollbar-both-edges", Variants = [], Important = false };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(2);

        var cssVar = nodes[0].ShouldBeOfType<Declaration>();
        cssVar.Property.ShouldBe("--tw-scrollbar-gutter-modifier");
        cssVar.Value.ShouldBe("both-edges");

        var property = nodes[1].ShouldBeOfType<Declaration>();
        property.Property.ShouldBe("scrollbar-gutter");
        property.Value.ShouldBe("stable var(--tw-scrollbar-gutter-modifier)");
    }

    [Fact]
    public void StaticCustomUtility_WithWrongCandidate_ShouldReturnFalse()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-thin",
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-width", "thin")
            )
        };

        var utility = new StaticCustomUtility(definition);
        var candidate = new StaticUtility { Raw = "scrollbar-thick", Root = "scrollbar-thick", Variants = [], Important = false }; // Wrong candidate

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeFalse();
        nodes.ShouldBeNull();
    }

    [Fact]
    public void StaticCustomUtility_WithWildcardPattern_ShouldThrowException()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-thumb-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-thumb-color", "--value()")
            )
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => new StaticCustomUtility(definition))
            .Message.ShouldContain("wildcard");
    }

    [Fact]
    public void StaticCustomUtility_MultipleNestedSelectors_ShouldGenerateMultipleNestedRules()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "custom-complex",
            Declarations = ImmutableList.Create(
                new CssDeclaration("display", "block")
            ),
            NestedSelectors = ImmutableList.Create(
                new NestedSelector(
                    "&:hover",
                    ImmutableList.Create(new CssDeclaration("color", "red"))
                ),
                new NestedSelector(
                    "&:focus",
                    ImmutableList.Create(new CssDeclaration("color", "blue"))
                ),
                new NestedSelector(
                    "&::before",
                    ImmutableList.Create(
                        new CssDeclaration("content", "''"),
                        new CssDeclaration("display", "inline-block")
                    )
                )
            )
        };

        var utility = new StaticCustomUtility(definition);
        var candidate = new StaticUtility { Raw = "custom-complex", Root = "custom-complex", Variants = [], Important = false };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(4); // 1 root declaration + 3 nested rules

        nodes[0].ShouldBeOfType<Declaration>().Property.ShouldBe("display");

        var hoverRule = nodes[1].ShouldBeOfType<NestedRule>();
        hoverRule.Selector.ShouldBe(":hover");
        hoverRule.Nodes[0].ShouldBeOfType<Declaration>().Value.ShouldBe("red");

        var focusRule = nodes[2].ShouldBeOfType<NestedRule>();
        focusRule.Selector.ShouldBe(":focus");
        focusRule.Nodes[0].ShouldBeOfType<Declaration>().Value.ShouldBe("blue");

        var beforeRule = nodes[3].ShouldBeOfType<NestedRule>();
        beforeRule.Selector.ShouldBe("::before");
        beforeRule.Nodes.Count.ShouldBe(2);
    }
}