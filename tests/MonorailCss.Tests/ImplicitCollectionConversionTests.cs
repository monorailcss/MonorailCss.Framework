using System.Collections.Immutable;
using MonorailCss.Parser.Custom;
using MonorailCss.Theme;
using Shouldly;

namespace MonorailCss.Tests;

/// <summary>
/// Verifies that the single-item implicit conversions on settings element
/// records compile and produce a one-element <see cref="ImmutableList{T}"/>
/// containing the original instance.
/// </summary>
public class ImplicitCollectionConversionTests
{
    [Fact]
    public void CssDeclaration_ImplicitlyWrapsAsOneElementList()
    {
        var declaration = new CssDeclaration("color", "red");

        ImmutableList<CssDeclaration> list = declaration;

        list.Count.ShouldBe(1);
        list[0].ShouldBeSameAs(declaration);
    }

    [Fact]
    public void NestedSelector_ImplicitlyWrapsAsOneElementList()
    {
        var nested = new NestedSelector("&::before", new CssDeclaration("content", "''"));

        ImmutableList<NestedSelector> list = nested;

        list.Count.ShouldBe(1);
        list[0].ShouldBeSameAs(nested);
    }

    [Fact]
    public void UtilityDefinition_ImplicitlyWrapsAsOneElementList()
    {
        var definition = new UtilityDefinition
        {
            Pattern = "demo",
            Declarations = new CssDeclaration("display", "block"),
        };

        ImmutableList<UtilityDefinition> list = definition;

        list.Count.ShouldBe(1);
        list[0].ShouldBeSameAs(definition);
    }

    [Fact]
    public void CustomVariantDefinition_ImplicitlyWrapsAsOneElementList()
    {
        var variant = new CustomVariantDefinition
        {
            Name = "demo",
            Selector = "&::demo",
        };

        ImmutableList<CustomVariantDefinition> list = variant;

        list.Count.ShouldBe(1);
        list[0].ShouldBeSameAs(variant);
    }

    [Fact]
    public void ProseDeclaration_ImplicitlyWrapsAsOneElementList()
    {
        var declaration = new ProseDeclaration { Property = "font-weight", Value = "700" };

        ImmutableList<ProseDeclaration> list = declaration;

        list.Count.ShouldBe(1);
        list[0].ShouldBeSameAs(declaration);
    }

    [Fact]
    public void ProseElementRule_ImplicitlyWrapsAsOneElementList()
    {
        var rule = new ProseElementRule
        {
            Selector = "a",
            Declarations = new ProseDeclaration { Property = "color", Value = "red" },
        };

        ImmutableList<ProseElementRule> list = rule;

        list.Count.ShouldBe(1);
        list[0].ShouldBeSameAs(rule);
    }
}
