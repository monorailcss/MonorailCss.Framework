using MonorailCss.Css;
using Shouldly;

namespace MonorailCss.Tests.Css;

public class AppliedSelectorTests
{
    [Fact]
    public void FromSelector_ShouldCreateWithNoWrappers()
    {
        var selector = Selector.FromClass("bg-red-500");
        var applied = AppliedSelector.FromSelector(selector);

        applied.Selector.ShouldBe(selector);
        applied.Wrappers.ShouldBeEmpty();
        applied.HasWrappers.ShouldBeFalse();
    }

    [Fact]
    public void FromClass_ShouldCreateFromClassName()
    {
        var applied = AppliedSelector.FromClass("hover\\:bg-red-500");

        applied.Selector.Value.ShouldBe(".hover\\:bg-red-500");
        applied.Wrappers.ShouldBeEmpty();
    }

    [Fact]
    public void WithWrapper_ShouldAddSingleWrapper()
    {
        var applied = AppliedSelector.FromClass("bg-red-500");
        var withMedia = applied.WithWrapper(AtRuleWrapper.Media("(min-width: 768px)"));

        withMedia.Wrappers.Length.ShouldBe(1);
        withMedia.Wrappers[0].Name.ShouldBe("media");
        withMedia.Wrappers[0].Params.ShouldBe("(min-width: 768px)");
        withMedia.HasWrappers.ShouldBeTrue();
    }

    [Fact]
    public void WithWrappers_ShouldAddMultipleWrappers()
    {
        var applied = AppliedSelector.FromClass("bg-red-500");
        var wrapped = applied.WithWrappers(
            AtRuleWrapper.Media("(min-width: 768px)"),
            AtRuleWrapper.Supports("(display: grid)")
        );

        wrapped.Wrappers.Length.ShouldBe(2);
        wrapped.Wrappers[0].Name.ShouldBe("media");
        wrapped.Wrappers[1].Name.ShouldBe("supports");
    }

    [Fact]
    public void WithSelector_ShouldReplaceSelector()
    {
        var applied = AppliedSelector.FromClass("bg-red-500")
            .WithWrapper(AtRuleWrapper.Media("(min-width: 768px)"));

        var newSelector = Selector.FromClass("bg-blue-500");
        var result = applied.WithSelector(newSelector);

        result.Selector.ShouldBe(newSelector);
        result.Wrappers.Length.ShouldBe(1);
    }

    [Fact]
    public void TransformSelector_ShouldApplyTransformation()
    {
        var applied = AppliedSelector.FromClass("bg-red-500");
        var transformed = applied.TransformSelector(s => s.WithPseudo(":hover"));

        transformed.Selector.Value.ShouldBe(".bg-red-500:hover");
    }

    [Fact]
    public void ToString_WithNoWrappers_ShouldReturnSelector()
    {
        var applied = AppliedSelector.FromClass("bg-red-500");
        applied.ToString().ShouldBe(".bg-red-500");
    }

    [Fact]
    public void ToString_WithWrappers_ShouldFormatCorrectly()
    {
        var applied = AppliedSelector.FromClass("bg-red-500")
            .WithWrapper(AtRuleWrapper.Media("(min-width: 768px)"));

        applied.ToString().ShouldBe("@media (min-width: 768px) { .bg-red-500 }");
    }

    [Fact]
    public void ToString_WithMultipleWrappers_ShouldFormatCorrectly()
    {
        var applied = AppliedSelector.FromClass("bg-red-500")
            .WithWrappers(
                AtRuleWrapper.Media("(min-width: 768px)"),
                AtRuleWrapper.Supports("(display: grid)")
            );

        applied.ToString().ShouldBe("@media (min-width: 768px) @supports (display: grid) { .bg-red-500 }");
    }
}

public class AtRuleWrapperTests
{
    [Fact]
    public void Media_ShouldCreateMediaWrapper()
    {
        var wrapper = AtRuleWrapper.Media("(min-width: 768px)");
        wrapper.Name.ShouldBe("media");
        wrapper.Params.ShouldBe("(min-width: 768px)");
        wrapper.ToCss().ShouldBe("@media (min-width: 768px)");
    }

    [Fact]
    public void Supports_ShouldCreateSupportsWrapper()
    {
        var wrapper = AtRuleWrapper.Supports("(display: grid)");
        wrapper.Name.ShouldBe("supports");
        wrapper.Params.ShouldBe("(display: grid)");
        wrapper.ToCss().ShouldBe("@supports (display: grid)");
    }

    [Fact]
    public void Container_ShouldCreateContainerWrapper()
    {
        var wrapper = AtRuleWrapper.Container("(min-width: 768px)");
        wrapper.Name.ShouldBe("container");
        wrapper.Params.ShouldBe("(min-width: 768px)");
        wrapper.ToCss().ShouldBe("@container (min-width: 768px)");
    }

    [Fact]
    public void Layer_ShouldCreateLayerWrapper()
    {
        var wrapper = AtRuleWrapper.Layer("utilities");
        wrapper.Name.ShouldBe("layer");
        wrapper.Params.ShouldBe("utilities");
        wrapper.ToCss().ShouldBe("@layer utilities");
    }

    [Fact]
    public void ToCss_WithEmptyParams_ShouldOmitParams()
    {
        var wrapper = new AtRuleWrapper("charset", "");
        wrapper.ToCss().ShouldBe("@charset");
    }

    [Fact]
    public void ToString_ShouldReturnToCss()
    {
        var wrapper = AtRuleWrapper.Media("screen");
        wrapper.ToString().ShouldBe("@media screen");
    }
}