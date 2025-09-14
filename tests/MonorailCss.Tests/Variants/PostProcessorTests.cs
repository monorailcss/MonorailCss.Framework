using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Processing;
using MonorailCss.Variants;
using Shouldly;

namespace MonorailCss.Tests.Variants;

public class PostProcessorTests
{
    private readonly PostProcessor _processor;

    public PostProcessorTests()
    {
        var registry = new VariantRegistry();
        registry.RegisterBuiltInVariants(MonorailCss.Theme.Theme.CreateWithDefaults());
        _processor = new PostProcessor(registry);
    }

    private Candidate CreateTestCandidate(string raw, string root, VariantToken[] variants, bool important = false)
    {
        return new StaticUtility
        {
            Raw = raw,
            Root = root,
            Variants = [.. variants],
            Important = important
        };
    }

    [Fact]
    public void ApplyPostProcessing_ShouldApplyVariantsInReverseOrder()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("background-color", "red")
        );

        var candidate = CreateTestCandidate(
            "hover:focus:bg-red-500",
            "bg-red-500",
            [
                new VariantToken("hover", null, null, "hover"),
                new VariantToken("focus", null, null, "focus"),
            ]
        );

        var result = _processor.ApplyPostProcessing(nodes, candidate);

        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();
        // Variants applied left-to-right: hover first, then focus
        styleRule.Selector.ShouldBe(".hover\\:focus\\:bg-red-500:hover:focus");
    }

    [Fact]
    public void ApplyPostProcessing_ShouldHandleImportantFlag()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("color", "blue")
        );

        var candidate = CreateTestCandidate(
            "!text-blue-500",
            "text-blue-500",
            [],
            true
        );

        var result = _processor.ApplyPostProcessing(nodes, candidate);

        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();
        var declaration = styleRule.Nodes[0].ShouldBeOfType<Declaration>();
        declaration.Important.ShouldBeTrue();
    }

    [Fact]
    public void ApplyPostProcessing_ShouldWrapWithAtRules()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("padding", "1rem")
        );

        var candidate = CreateTestCandidate(
            "sm:p-4",
            "p-4",
            [
                new VariantToken("sm", null, null, "sm"),
            ]
        );

        var result = _processor.ApplyPostProcessing(nodes, candidate);

        result.Count.ShouldBe(1);
        var atRule = result[0].ShouldBeOfType<AtRule>();
        atRule.Name.ShouldBe("media");
        atRule.Params.ShouldBe("(min-width: 640px)");

        var styleRule = atRule.Nodes[0].ShouldBeOfType<StyleRule>();
        styleRule.Selector.ShouldBe(".sm\\:p-4");
    }

    [Fact]
    public void ApplyPostProcessing_ShouldStackMultipleAtRules()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("display", "none")
        );

        var candidate = CreateTestCandidate(
            "sm:motion-safe:hidden",
            "hidden",
            [
                new VariantToken("sm", null, null, "sm"),
                new VariantToken("motion-safe", null, null, "motion-safe"),
            ]
        );

        var result = _processor.ApplyPostProcessing(nodes, candidate);

        result.Count.ShouldBe(1);

        // Outer wrapper should be sm (applied last)
        var outerAtRule = result[0].ShouldBeOfType<AtRule>();
        outerAtRule.Name.ShouldBe("media");
        outerAtRule.Params.ShouldBe("(min-width: 640px)");

        // Inner wrapper should be motion-safe (applied first)
        var innerAtRule = outerAtRule.Nodes[0].ShouldBeOfType<AtRule>();
        innerAtRule.Name.ShouldBe("media");
        innerAtRule.Params.ShouldBe("(prefers-reduced-motion: no-preference)");

        var styleRule = innerAtRule.Nodes[0].ShouldBeOfType<StyleRule>();
        styleRule.Selector.ShouldBe(".sm\\:motion-safe\\:hidden");
    }

    [Fact]
    public void ApplyPostProcessing_ShouldEscapeClassName()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("width", "50%")
        );

        var candidate = CreateTestCandidate(
            "w-1/2",
            "w-1/2",
            []
        );

        var result = _processor.ApplyPostProcessing(nodes, candidate);

        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();
        styleRule.Selector.ShouldBe(".w-1\\/2");
    }

    [Fact]
    public void ApplyPostProcessing_ShouldHandleArbitraryVariants()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("text-decoration", "underline")
        );

        var candidate = CreateTestCandidate(
            "[&>*]:underline",
            "underline",
            [
                VariantToken.Arbitrary("&>*"),
            ]
        );

        var result = _processor.ApplyPostProcessing(nodes, candidate);

        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();
        // The arbitrary variant should transform the selector
        styleRule.Selector.ShouldContain(">");
    }

    [Fact]
    public void ApplyPostProcessing_ShouldHandleGroupVariants()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("color", "blue")
        );

        var candidate = CreateTestCandidate(
            "group-hover:text-blue-500",
            "text-blue-500",
            [
                new VariantToken("group", null, "hover", "group-hover"),
            ]
        );

        var result = _processor.ApplyPostProcessing(nodes, candidate);

        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();
        styleRule.Selector.ShouldBe(".group-hover\\:text-blue-500:is(:where(.group):hover *)");
    }

    [Fact]
    public void ApplyPostProcessing_ShouldHandlePeerVariants()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("border-color", "red")
        );

        var candidate = CreateTestCandidate(
            "peer-focus:border-red-500",
            "border-red-500",
            [
                new VariantToken("peer", null, "focus", "peer-focus"),
            ]
        );

        var result = _processor.ApplyPostProcessing(nodes, candidate);

        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();
        styleRule.Selector.ShouldBe(".peer-focus\\:border-red-500:is(:where(.peer):focus ~ *)");
    }

    [Fact]
    public void ApplyPostProcessing_ShouldHandleComplexVariantStack()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("color", "white")
        );

        var candidate = CreateTestCandidate(
            "dark:hover:sm:text-white",
            "text-white",
            [
                new VariantToken("dark", null, null, "dark"),
                new VariantToken("hover", null, null, "hover"),
                new VariantToken("sm", null, null, "sm"),
            ]
        );

        var result = _processor.ApplyPostProcessing(nodes, candidate);

        result.Count.ShouldBe(1);

        // sm creates the outer at-rule
        var atRule = result[0].ShouldBeOfType<AtRule>();
        atRule.Name.ShouldBe("media");
        atRule.Params.ShouldBe("(min-width: 640px)");

        // Inside should be a style rule with hover and dark applied
        var styleRule = atRule.Nodes[0].ShouldBeOfType<StyleRule>();
        // Applied left-to-right: dark first, then hover
        styleRule.Selector.ShouldBe(".dark\\:hover\\:sm\\:text-white:where(.dark, .dark *):hover");
    }

    [Fact]
    public void ApplyPostProcessing_ShouldPreserveDeclarationOrder()
    {
        var nodes = ImmutableList.Create<AstNode>(
            new Declaration("background-color", "red"),
            new Declaration("color", "white"),
            new Declaration("padding", "1rem")
        );

        var candidate = CreateTestCandidate(
            "hover:card",
            "card",
            [
                new VariantToken("hover", null, null, "hover"),
            ]
        );

        var result = _processor.ApplyPostProcessing(nodes, candidate);

        result.Count.ShouldBe(1);
        var styleRule = result[0].ShouldBeOfType<StyleRule>();
        styleRule.Nodes.Count.ShouldBe(3);

        // Verify order is preserved
        var decl1 = styleRule.Nodes[0].ShouldBeOfType<Declaration>();
        decl1.Property.ShouldBe("background-color");

        var decl2 = styleRule.Nodes[1].ShouldBeOfType<Declaration>();
        decl2.Property.ShouldBe("color");

        var decl3 = styleRule.Nodes[2].ShouldBeOfType<Declaration>();
        decl3.Property.ShouldBe("padding");
    }
}