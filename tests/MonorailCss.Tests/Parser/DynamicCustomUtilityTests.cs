using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Parser.Custom;
using MonorailCss.Utilities;
using Shouldly;

namespace MonorailCss.Tests.Parser;

/// <summary>
/// Unit tests for the DynamicCustomUtility class.
/// </summary>
public class DynamicCustomUtilityTests
{
    private readonly MonorailCss.Theme.Theme _theme;

    public DynamicCustomUtilityTests()
    {
        _theme = new MonorailCss.Theme.Theme();
        // Add some test theme values
        _theme = _theme.Add("--color-red-500", "#ef4444");
        _theme = _theme.Add("--color-blue-500", "#3b82f6");
        _theme = _theme.Add("--color-green-500", "#10b981");
        _theme = _theme.Add("--spacing-4", "1rem");
        _theme = _theme.Add("--spacing-8", "2rem");
    }

    [Fact]
    public void Constructor_WithNullDefinition_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new DynamicCustomUtility(null!));
    }

    [Fact]
    public void Constructor_WithoutWildcard_ThrowsArgumentException()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-none",
            IsWildcard = false,
            Declarations = ImmutableList<CssDeclaration>.Empty
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => new DynamicCustomUtility(definition));
    }

    [Fact]
    public void Constructor_WithWildcard_CreatesInstance()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-thumb-*",
            IsWildcard = true,
            Declarations = ImmutableList<CssDeclaration>.Empty
        };

        // Act
        var utility = new DynamicCustomUtility(definition);

        // Assert
        utility.ShouldNotBeNull();
        utility.Priority.ShouldBe(UtilityPriority.NamespaceHandler);
        utility.Layer.ShouldBe(UtilityLayer.Utility);
    }

    [Fact]
    public void GetFunctionalRoots_ReturnsBasePattern()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-thumb-*",
            IsWildcard = true,
            Declarations = ImmutableList<CssDeclaration>.Empty
        };
        var utility = new DynamicCustomUtility(definition);

        // Act
        var roots = utility.GetFunctionalRoots();

        // Assert
        roots.ShouldBe(["scrollbar-thumb"]);
    }

    [Fact]
    public void TryCompile_WithMatchingPattern_ReturnsTrue()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "test-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("test-property", "*")
            )
        };
        var utility = new DynamicCustomUtility(definition);
        var candidate = new FunctionalUtility
        {
            Raw = "test-value",
            Variants = ImmutableArray<MonorailCss.Variants.VariantToken>.Empty,
            Root = "test",
            Value = CandidateValue.Named("value")
        };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);
        var declaration = nodes[0] as Declaration;
        declaration.ShouldNotBeNull();
        declaration.Property.ShouldBe("test-property");
        declaration.Value.ShouldBe("value");
    }

    [Fact]
    public void TryCompile_WithNonMatchingPattern_ReturnsFalse()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "test-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("test-property", "*")
            )
        };
        var utility = new DynamicCustomUtility(definition);
        var candidate = new FunctionalUtility
        {
            Raw = "other-value",
            Variants = ImmutableArray<MonorailCss.Variants.VariantToken>.Empty,
            Root = "other",
            Value = CandidateValue.Named("value")
        };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeFalse();
        nodes.ShouldBeNull();
    }

    [Fact]
    public void TryCompile_WithValueFunction_ResolvesThemeValue()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-thumb-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-thumb-color", "--value(--color-*)")
            )
        };
        var utility = new DynamicCustomUtility(definition);
        var candidate = new FunctionalUtility
        {
            Raw = "scrollbar-thumb-red-500",
            Variants = ImmutableArray<MonorailCss.Variants.VariantToken>.Empty,
            Root = "scrollbar-thumb",
            Value = CandidateValue.Named("red-500")
        };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);
        var declaration = nodes[0] as Declaration;
        declaration.ShouldNotBeNull();
        declaration.Property.ShouldBe("--tw-scrollbar-thumb-color");
        declaration.Value.ShouldBe("var(--color-red-500)");
    }

    [Fact]
    public void TryCompile_WithArbitraryValue_ReturnsArbitraryValue()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "scrollbar-thumb-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-thumb-color", "--value(--color-*)")
            )
        };
        var utility = new DynamicCustomUtility(definition);
        var candidate = new FunctionalUtility
        {
            Raw = "scrollbar-thumb-[#123456]",
            Variants = ImmutableArray<MonorailCss.Variants.VariantToken>.Empty,
            Root = "scrollbar-thumb",
            Value = CandidateValue.Arbitrary("#123456")
        };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);
        var declaration = nodes[0] as Declaration;
        declaration.ShouldNotBeNull();
        declaration.Property.ShouldBe("--tw-scrollbar-thumb-color");
        declaration.Value.ShouldBe("#123456");
    }

    [Fact]
    public void TryCompile_WithNestedSelectors_GeneratesNestedRules()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "test-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("color", "*")
            ),
            NestedSelectors = ImmutableList.Create(
                new NestedSelector("&:hover", ImmutableList.Create(
                    new CssDeclaration("background-color", "*")
                ))
            )
        };
        var utility = new DynamicCustomUtility(definition);
        var candidate = new FunctionalUtility
        {
            Raw = "test-blue",
            Variants = ImmutableArray<MonorailCss.Variants.VariantToken>.Empty,
            Root = "test",
            Value = CandidateValue.Named("blue")
        };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(2);

        // Check root declaration
        var declaration = nodes[0] as Declaration;
        declaration.ShouldNotBeNull();
        declaration.Property.ShouldBe("color");
        declaration.Value.ShouldBe("blue");

        // Check nested rule
        var nestedRule = nodes[1] as NestedRule;
        nestedRule.ShouldNotBeNull();
        nestedRule.Selector.ShouldBe(":hover");
        nestedRule.Nodes.Count.ShouldBe(1);
        var nestedDecl = nestedRule.Nodes[0] as Declaration;
        nestedDecl.ShouldNotBeNull();
        nestedDecl.Property.ShouldBe("background-color");
        nestedDecl.Value.ShouldBe("blue");
    }

    [Fact]
    public void TryCompile_WithImportantFlag_AppliesImportant()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "test-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("color", "*")
            )
        };
        var utility = new DynamicCustomUtility(definition);
        var candidate = new FunctionalUtility
        {
            Raw = "test-red",
            Variants = ImmutableArray<MonorailCss.Variants.VariantToken>.Empty,
            Root = "test",
            Value = CandidateValue.Named("red"),
            Important = true
        };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);
        var declaration = nodes[0] as Declaration;
        declaration.ShouldNotBeNull();
        declaration.Important.ShouldBeTrue();
    }

    [Fact]
    public void TryCompile_WithStaticUtilityMatchingPattern_Works()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "test-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("test-property", "*")
            )
        };
        var utility = new DynamicCustomUtility(definition);
        var candidate = new StaticUtility
        {
            Raw = "test-value",
            Variants = ImmutableArray<MonorailCss.Variants.VariantToken>.Empty,
            Root = "test-value"
        };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);
        var declaration = nodes[0] as Declaration;
        declaration.ShouldNotBeNull();
        declaration.Property.ShouldBe("test-property");
        declaration.Value.ShouldBe("value");
    }

    [Fact]
    public void TryCompile_WithComplexWildcardPattern_ExtractsCorrectValue()
    {
        // Arrange
        var definition = new UtilityDefinition
        {
            Pattern = "shadow-*-sm",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("box-shadow", "0 1px 2px 0 *")
            )
        };
        var utility = new DynamicCustomUtility(definition);
        var candidate = new FunctionalUtility
        {
            Raw = "shadow-red-sm",
            Variants = ImmutableArray<MonorailCss.Variants.VariantToken>.Empty,
            Root = "shadow",
            Value = CandidateValue.Named("red-sm")
        };

        // Act
        var result = utility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);
        var declaration = nodes[0] as Declaration;
        declaration.ShouldNotBeNull();
        declaration.Property.ShouldBe("box-shadow");
        declaration.Value.ShouldBe("0 1px 2px 0 red");
    }
}