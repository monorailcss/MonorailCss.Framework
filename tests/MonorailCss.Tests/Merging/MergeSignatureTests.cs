using MonorailCss.Merging;
using Shouldly;

namespace MonorailCss.Tests.Merging;

public class MergeSignatureTests
{
    private static readonly SignatureBuilder _builder = CreateBuilder();

    private static SignatureBuilder CreateBuilder()
    {
        var framework = new CssFramework(new CssFrameworkSettings { IncludePreflight = false });
        return new SignatureBuilder(framework.Parser, framework.UtilityRegistry, framework.Settings.Theme);
    }

    [Fact]
    public void Shorthand_ExpandsToLonghands()
    {
        var signature = _builder.ComputeSignature("p-4");
        signature.ShouldNotBeNull();
        signature.Writes.ShouldContain("padding");
        signature.Writes.ShouldContain("padding-top");
        signature.Writes.ShouldContain("padding-inline-start");
    }

    [Fact]
    public void AxisShorthand_DoesNotCoverTheFullShorthand()
    {
        var signature = _builder.ComputeSignature("px-2");
        signature.ShouldNotBeNull();
        signature.Writes.ShouldContain("padding-inline");
        signature.Writes.ShouldContain("padding-left");
        signature.Writes.ShouldNotContain("padding");
        signature.Writes.ShouldNotContain("padding-top");
    }

    [Fact]
    public void ComposableUtility_ScaffoldDeclarationIsExcluded()
    {
        var signature = _builder.ComputeSignature("touch-pan-x");
        signature.ShouldNotBeNull();
        signature.Writes.ShouldBe(["--tw-pan-x"], ignoreOrder: true);
    }

    [Fact]
    public void ResetUtility_CoversComposableSiblings()
    {
        var signature = _builder.ComputeSignature("touch-none");
        signature.ShouldNotBeNull();
        signature.Writes.ShouldContain("touch-action");
        signature.Covers.ShouldContain("--tw-pan-x");
        signature.Covers.ShouldContain("--tw-pinch-zoom");
    }

    [Fact]
    public void FontSize_KeepsLineHeightAndCoversLeading()
    {
        var signature = _builder.ComputeSignature("text-lg");
        signature.ShouldNotBeNull();
        signature.Writes.ShouldContain("font-size");
        signature.Writes.ShouldContain("line-height");
        signature.Covers.ShouldContain("--tw-leading");
    }

    [Fact]
    public void TextColor_DoesNotCoverLeading()
    {
        var signature = _builder.ComputeSignature("text-red-500");
        signature.ShouldNotBeNull();
        signature.Writes.ShouldContain("color");
        signature.Covers.ShouldBeEmpty();
    }

    [Fact]
    public void ArbitraryProperty_WritesThePropertyDirectly()
    {
        var signature = _builder.ComputeSignature("[padding:1rem]");
        signature.ShouldNotBeNull();
        signature.Writes.ShouldContain("padding");
        signature.Writes.ShouldContain("padding-left");
    }

    [Theory]
    [InlineData("definitely-not-a-class")]
    [InlineData("prose")]
    public void UnknownOrComponentClasses_ArePassthrough(string token)
    {
        _builder.ComputeSignature(token).ShouldBeNull();
    }

    [Fact]
    public void VariantKey_IsOrderNormalized()
    {
        var first = _builder.ComputeSignature("hover:focus:p-4");
        var second = _builder.ComputeSignature("focus:hover:p-4");
        first.ShouldNotBeNull();
        second.ShouldNotBeNull();
        first.VariantKey.ShouldBe(second.VariantKey);
    }

    [Fact]
    public void VariantKey_PreservesPseudoElementPosition()
    {
        var first = _builder.ComputeSignature("before:hover:p-4");
        var second = _builder.ComputeSignature("hover:before:p-4");
        first.ShouldNotBeNull();
        second.ShouldNotBeNull();
        first.VariantKey.ShouldNotBe(second.VariantKey);
    }

    [Fact]
    public void ImportantFlag_IsPartOfTheSignature()
    {
        var important = _builder.ComputeSignature("p-4!");
        var normal = _builder.ComputeSignature("p-4");
        important.ShouldNotBeNull();
        normal.ShouldNotBeNull();
        important.Important.ShouldBeTrue();
        normal.Important.ShouldBeFalse();
        important.Writes.ShouldBe(normal.Writes, ignoreOrder: true);
    }
}
