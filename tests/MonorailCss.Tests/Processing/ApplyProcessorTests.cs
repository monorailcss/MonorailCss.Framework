using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Css;
using MonorailCss.Processing;
using MonorailCss.Theme;
using MonorailCss.Variants;
using Shouldly;

namespace MonorailCss.Tests.Processing;

public class ApplyProcessorTests
{
    private readonly UtilityRegistry _utilityRegistry;
    private readonly ApplyProcessor _processor;
    private readonly CssPropertyRegistry _propertyRegistry;
    private readonly MonorailCss.Theme.Theme _theme;

    public ApplyProcessorTests()
    {
        _utilityRegistry = new UtilityRegistry(autoRegisterUtilities: true);
        _theme = new MonorailCss.Theme.Theme();

        var variantRegistry = new VariantRegistry();
        variantRegistry.RegisterBuiltInVariants(_theme);

        _processor = new ApplyProcessor(_utilityRegistry);
        _propertyRegistry = new CssPropertyRegistry();
    }

    [Fact]
    public void ProcessApplies_WithEmptyDictionary_ReturnsEmptyList()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty;

        // Act
        var result = _processor.ProcessApplies(applies, _utilityRegistry, _theme, _propertyRegistry, new ThemeUsageTracker(_theme));

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void ProcessApplies_WithSingleUtility_GeneratesStyleRule()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "block");

        // Act
        var result = _processor.ProcessApplies(applies, _utilityRegistry, _theme, _propertyRegistry, new ThemeUsageTracker(_theme));

        // Assert
        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();
        styleRule.Selector.ShouldBe(".btn");
        styleRule.Nodes.Count.ShouldBe(1);

        var declaration = styleRule.Nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("display");
        declaration.Value.ShouldBe("block");
    }

    [Fact]
    public void ProcessApplies_WithMultipleUtilities_GeneratesMultipleDeclarations()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "block text-red-500");

        // Act
        var result = _processor.ProcessApplies(applies, _utilityRegistry, _theme, _propertyRegistry, new ThemeUsageTracker(_theme));

        // Assert
        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();
        styleRule.Selector.ShouldBe(".btn");
        styleRule.Nodes.Count.ShouldBe(2);

        // Check for display: block
        var displayDecl = styleRule.Nodes
            .OfType<Declaration>()
            .FirstOrDefault(d => d.Property == "display");
        displayDecl.ShouldNotBeNull();
        displayDecl.Value.ShouldBe("block");

        // Check for color property
        var colorDecl = styleRule.Nodes
            .OfType<Declaration>()
            .FirstOrDefault(d => d.Property == "color");
        colorDecl.ShouldNotBeNull();
        colorDecl.Value.ShouldContain("--color-red-500");
    }

    [Fact]
    public void ProcessApplies_WithBackgroundUtility_GeneratesCorrectDeclaration()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".card", "bg-blue-500");

        // Act
        var result = _processor.ProcessApplies(applies, _utilityRegistry, _theme, _propertyRegistry, new ThemeUsageTracker(_theme));

        // Assert
        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();
        styleRule.Selector.ShouldBe(".card");

        var bgDecl = styleRule.Nodes
            .OfType<Declaration>()
            .FirstOrDefault(d => d.Property == "background-color");
        bgDecl.ShouldNotBeNull();
        bgDecl.Value.ShouldContain("--color-blue-500");
    }

    [Fact]
    public void ProcessApplies_WithDuplicateProperties_LastOneWins()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "bg-blue-500 bg-red-500");

        // Act
        var result = _processor.ProcessApplies(applies, _utilityRegistry, _theme, _propertyRegistry, new ThemeUsageTracker(_theme));

        // Assert
        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();

        // Should only have one background-color declaration
        var bgDeclarations = styleRule.Nodes
            .OfType<Declaration>()
            .Where(d => d.Property == "background-color")
            .ToList();

        bgDeclarations.Count.ShouldBe(1);
        bgDeclarations[0].Value.ShouldContain("--color-red-500"); // Last one wins
    }

    [Fact]
    public void ProcessApplies_WithMultipleApplies_GeneratesMultipleStyleRules()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "block")
            .Add(".card", "flex");

        // Act
        var result = _processor.ProcessApplies(applies, _utilityRegistry, _theme, _propertyRegistry, new ThemeUsageTracker(_theme));

        // Assert
        result.Count.ShouldBe(2);

        var btnRule = result.OfType<StyleRule>().FirstOrDefault(r => r.Selector == ".btn");
        btnRule.ShouldNotBeNull();
        var btnDecl = btnRule.Nodes[0].ShouldBeOfType<Declaration>();
        btnDecl.Property.ShouldBe("display");
        btnDecl.Value.ShouldBe("block");

        var cardRule = result.OfType<StyleRule>().FirstOrDefault(r => r.Selector == ".card");
        cardRule.ShouldNotBeNull();
        var cardDecl = cardRule.Nodes[0].ShouldBeOfType<Declaration>();
        cardDecl.Property.ShouldBe("display");
        cardDecl.Value.ShouldBe("flex");
    }

    [Fact]
    public void ProcessApplies_WithSpecialValues_HandlesCorrectly()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".transparent", "bg-transparent");

        // Act
        var result = _processor.ProcessApplies(applies, _utilityRegistry, _theme, _propertyRegistry, new ThemeUsageTracker(_theme));

        // Assert
        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();

        var bgDecl = styleRule.Nodes
            .OfType<Declaration>()
            .FirstOrDefault(d => d.Property == "background-color");
        bgDecl.ShouldNotBeNull();
        bgDecl.Value.ShouldBe("transparent");
    }

    [Fact]
    public void ProcessApplies_WithInvalidUtility_IgnoresIt()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "block invalid-utility flex");

        // Act
        var result = _processor.ProcessApplies(applies, _utilityRegistry, _theme, _propertyRegistry, new ThemeUsageTracker(_theme));

        // Assert
        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();

        // Should have one declaration (flex overwrites block), invalid-utility is ignored
        styleRule.Nodes.Count.ShouldBe(1);

        var declaration = styleRule.Nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("display");
        declaration.Value.ShouldBe("flex"); // Last one wins
    }

    [Fact]
    public void ProcessApplies_WithEmptyUtilityString_SkipsApply()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "");

        // Act
        var result = _processor.ProcessApplies(applies, _utilityRegistry, _theme, _propertyRegistry, new ThemeUsageTracker(_theme));

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public void ProcessApplies_WithWhitespaceOnlyUtilityString_SkipsApply()
    {
        // Arrange
        var applies = ImmutableDictionary<string, string>.Empty
            .Add(".btn", "   ");

        // Act
        var result = _processor.ProcessApplies(applies, _utilityRegistry, _theme, _propertyRegistry, new ThemeUsageTracker(_theme));

        // Assert
        result.ShouldBeEmpty();
    }
}