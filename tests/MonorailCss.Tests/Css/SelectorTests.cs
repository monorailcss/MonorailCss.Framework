using MonorailCss.Css;
using Shouldly;

namespace MonorailCss.Tests.Css;

public class SelectorTests
{
    [Fact]
    public void FromClass_ShouldCreateClassSelector()
    {
        var selector = Selector.FromClass("hover\\:bg-red-500");
        selector.Value.ShouldBe(".hover\\:bg-red-500");
    }

    [Fact]
    public void WithPseudo_ShouldAppendPseudoClass()
    {
        var selector = Selector.FromClass("bg-red-500");
        var result = selector.WithPseudo(":hover");
        result.Value.ShouldBe(".bg-red-500:hover");
    }

    [Fact]
    public void WithPseudo_ShouldHandleMultiplePseudos()
    {
        var selector = Selector.FromClass("bg-red-500");
        var result = selector
            .WithPseudo(":hover")
            .WithPseudo(":focus");
        result.Value.ShouldBe(".bg-red-500:hover:focus");
    }

    [Fact]
    public void Relativize_ShouldReplaceAmpersand()
    {
        var selector = Selector.FromClass("container");
        var result = selector.Relativize("[&>*]");
        result.Value.ShouldBe("[.container>*]");
    }

    [Fact]
    public void Relativize_ShouldHandleMultipleAmpersands()
    {
        var selector = Selector.FromClass("group");
        var result = selector.Relativize("& > & + &");
        result.Value.ShouldBe(".group > .group + .group");
    }

    [Fact]
    public void DescendantOf_ShouldCreateDescendantSelector()
    {
        var selector = Selector.FromClass("btn");
        var result = selector.DescendantOf(".dark");
        result.Value.ShouldBe(".dark .btn");
    }

    [Fact]
    public void SiblingOf_ShouldCreateSiblingSelector()
    {
        var selector = Selector.FromClass("text");
        var result = selector.SiblingOf(".checkbox", "~");
        result.Value.ShouldBe(".checkbox ~ .text");
    }

    [Fact]
    public void SiblingOf_ShouldHandleDirectSibling()
    {
        var selector = Selector.FromClass("text");
        var result = selector.SiblingOf(".checkbox", "+");
        result.Value.ShouldBe(".checkbox + .text");
    }

    [Fact]
    public void WithClassPrefix_ShouldAddPrefix()
    {
        var selector = Selector.FromClass("bg-blue-500");
        var result = selector.WithClassPrefix(".group:hover");
        result.Value.ShouldBe(".group:hover .bg-blue-500");
    }

    [Fact]
    public void Combine_ShouldCombineSelectorsDirectly()
    {
        var selector = Selector.FromClass("btn");
        var result = selector.Combine(".active");
        result.Value.ShouldBe(".btn.active");
    }

    [Fact]
    public void Combine_ShouldCombineWithCombinator()
    {
        var selector = Selector.FromClass("parent");
        var result = selector.Combine(".child", " > ");
        result.Value.ShouldBe(".parent > .child");
    }

    [Fact]
    public void WithAttribute_ShouldAddAttributeSelector()
    {
        var selector = Selector.FromClass("text-left");
        var result = selector.WithAttribute("[dir=\"rtl\"]");
        result.Value.ShouldBe("[dir=\"rtl\"] .text-left");
    }

    [Fact]
    public void InWhere_ShouldWrapInWhere()
    {
        var selector = Selector.FromClass("btn");
        var result = selector.InWhere();
        result.Value.ShouldBe(":where(.btn)");
    }

    [Fact]
    public void InIs_ShouldWrapInIs()
    {
        var selector = Selector.FromClass("btn");
        var result = selector.InIs();
        result.Value.ShouldBe(":is(.btn)");
    }

    [Fact]
    public void InNot_ShouldWrapInNot()
    {
        var selector = Selector.FromClass("btn");
        var result = selector.InNot();
        result.Value.ShouldBe(":not(.btn)");
    }

    [Fact]
    public void ComplexComposition_ShouldWork()
    {
        var selector = Selector.FromClass("bg-red-500");
        var result = selector
            .WithPseudo(":hover")
            .DescendantOf(".dark")
            .WithAttribute("[data-theme=\"dark\"]");

        result.Value.ShouldBe("[data-theme=\"dark\"] .dark .bg-red-500:hover");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var selector = new Selector(".test-class");
        selector.ToString().ShouldBe(".test-class");
    }
}