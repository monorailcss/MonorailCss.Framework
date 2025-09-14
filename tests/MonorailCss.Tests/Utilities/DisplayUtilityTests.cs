using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities;
using MonorailCss.Utilities.Layout;
using MonorailCss.Variants;
using Shouldly;

namespace MonorailCss.Tests.Utilities;

public class DisplayUtilityTests
{
    private readonly DisplayUtility _displayUtility;
    private readonly MonorailCss.Theme.Theme _theme;

    public DisplayUtilityTests()
    {
        _displayUtility = new DisplayUtility();
        _theme = new MonorailCss.Theme.Theme();
    }

    [Theory]
    [InlineData("block", "display", "block")]
    [InlineData("inline", "display", "inline")]
    [InlineData("inline-block", "display", "inline-block")]
    [InlineData("flex", "display", "flex")]
    [InlineData("inline-flex", "display", "inline-flex")]
    [InlineData("grid", "display", "grid")]
    [InlineData("inline-grid", "display", "inline-grid")]
    [InlineData("table", "display", "table")]
    [InlineData("flow-root", "display", "flow-root")]
    [InlineData("contents", "display", "contents")]
    [InlineData("list-item", "display", "list-item")]
    [InlineData("none", "display", "none")]
    [InlineData("hidden", "display", "none")]
    public void TryCompile_WithValidDisplayUtility_GeneratesCorrectCss(string utility, string expectedProperty, string expectedValue)
    {
        // Arrange
        var candidate = new StaticUtility
        {
            Raw = utility,
            Root = utility,
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        // Act
        var result = _displayUtility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe(expectedProperty);
        declaration.Value.ShouldBe(expectedValue);
        declaration.Important.ShouldBeFalse();
    }

    [Fact]
    public void TryCompile_WithImportantUtility_GeneratesImportantDeclaration()
    {
        // Arrange
        var candidate = new StaticUtility
        {
            Raw = "!flex",
            Root = "flex",
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = true
        };

        // Act
        var result = _displayUtility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeTrue();
        nodes.ShouldNotBeNull();
        nodes.Count.ShouldBe(1);

        var declaration = nodes[0].ShouldBeOfType<Declaration>();
        declaration.Property.ShouldBe("display");
        declaration.Value.ShouldBe("flex");
        declaration.Important.ShouldBeTrue();
    }

    [Fact]
    public void TryCompile_WithUnknownUtility_ReturnsFalse()
    {
        // Arrange
        var candidate = new StaticUtility
        {
            Raw = "unknown",
            Root = "unknown",
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        // Act
        var result = _displayUtility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeFalse();
        nodes.ShouldBeNull();
    }

    [Fact]
    public void TryCompile_WithFunctionalUtility_ReturnsFalse()
    {
        // Arrange
        var candidate = new FunctionalUtility
        {
            Raw = "bg-red-500",
            Root = "bg",
            Value = CandidateValue.Named("red-500"),
            Variants = ImmutableArray<VariantToken>.Empty,
            Important = false
        };

        // Act
        var result = _displayUtility.TryCompile(candidate, _theme, out var nodes);

        // Assert
        result.ShouldBeFalse();
        nodes.ShouldBeNull();
    }


    [Fact]
    public void Priority_ReturnsExactStatic()
    {
        // Act
        var priority = _displayUtility.Priority;

        // Assert
        priority.ShouldBe(UtilityPriority.ExactStatic);
    }

    [Fact]
    public void GetStaticUtilityNames_ReturnsAllDisplayUtilities()
    {
        // Act
        var names = new DisplayUtility().GetUtilityNames().ToImmutableHashSet();

        // Assert
        names.ShouldContain("block");
        names.ShouldContain("inline");
        names.ShouldContain("flex");
        names.ShouldContain("grid");
        names.ShouldContain("hidden");
        names.ShouldContain("none");
        names.Count.ShouldBe(22); // All display utilities
    }
}