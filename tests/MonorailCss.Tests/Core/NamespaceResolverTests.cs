using MonorailCss.Core;

namespace MonorailCss.Tests.Core;

public class NamespaceResolverTests
{
    [Fact]
    public void ColorNamespaceChains_ShouldHaveCorrectFallbacks()
    {
        // Test core color chains have --color as fallback
        Assert.Equal(["--background-color", "--color"], NamespaceResolver.BackgroundColorChain);
        Assert.Equal(["--text-color", "--color"], NamespaceResolver.TextColorChain);
        Assert.Equal(["--border-color", "--color"], NamespaceResolver.BorderColorChain);
        Assert.Equal(["--outline-color", "--color"], NamespaceResolver.OutlineColorChain);
        Assert.Equal(["--ring-color", "--color"], NamespaceResolver.RingColorChain);
        Assert.Equal(["--ring-offset-color", "--color"], NamespaceResolver.RingOffsetColorChain);
        Assert.Equal(["--accent-color", "--color"], NamespaceResolver.AccentColorChain);
        Assert.Equal(["--caret-color", "--color"], NamespaceResolver.CaretColorChain);
        Assert.Equal(["--fill", "--color"], NamespaceResolver.FillColorChain);
        Assert.Equal(["--stroke", "--color"], NamespaceResolver.StrokeColorChain);
        Assert.Equal(["--placeholder-color", "--color"], NamespaceResolver.PlaceholderColorChain);
        Assert.Equal(["--text-decoration-color", "--color"], NamespaceResolver.DecorationColorChain);

        // Test complex color chains
        Assert.Equal(["--divide-color", "--border-color", "--color"], NamespaceResolver.DivideColorChain);
    }

    [Fact]
    public void SpacingNamespaceChains_ShouldHaveCorrectFallbacks()
    {
        // Test spacing chains have --spacing as fallback
        Assert.Equal(["--padding", "--spacing"], NamespaceResolver.PaddingChain);
        Assert.Equal(["--margin", "--spacing"], NamespaceResolver.MarginChain);
        Assert.Equal(["--space", "--spacing"], NamespaceResolver.SpaceChain);
        Assert.Equal(["--gap", "--spacing"], NamespaceResolver.GapChain);
        Assert.Equal(["--scroll-margin", "--spacing"], NamespaceResolver.ScrollMarginChain);
        Assert.Equal(["--scroll-padding", "--spacing"], NamespaceResolver.ScrollPaddingChain);
        Assert.Equal(["--text-indent", "--spacing"], NamespaceResolver.TextIndentChain);
        Assert.Equal(["--translate", "--spacing"], NamespaceResolver.TranslateChain);
    }

    [Fact]
    public void SizingNamespaceChains_ShouldHaveCorrectFallbacks()
    {
        // Test sizing chains
        Assert.Equal(["--width", "--spacing"], NamespaceResolver.WidthChain);
        Assert.Equal(["--height", "--spacing"], NamespaceResolver.HeightChain);
        Assert.Equal(["--min-width", "--width", "--spacing"], NamespaceResolver.MinWidthChain);
        Assert.Equal(["--min-height", "--height", "--spacing"], NamespaceResolver.MinHeightChain);
        Assert.Equal(["--max-width", "--width"], NamespaceResolver.MaxWidthChain);
        Assert.Equal(["--max-height", "--height"], NamespaceResolver.MaxHeightChain);
        Assert.Equal(["--flex-basis", "--spacing"], NamespaceResolver.FlexBasisChain);
    }

    [Fact]
    public void BorderWidthNamespaceChains_ShouldHaveCorrectFallbacks()
    {
        // Test border width chains
        Assert.Equal(["--border-width"], NamespaceResolver.BorderWidthChain);
        Assert.Equal(["--outline-width", "--border-width"], NamespaceResolver.OutlineWidthChain);
        Assert.Equal(["--outline-offset", "--spacing"], NamespaceResolver.OutlineOffsetChain);
        Assert.Equal(["--ring-width", "--border-width", "--spacing"], NamespaceResolver.RingWidthChain);
        Assert.Equal(["--ring-offset-width", "--border-width", "--spacing"], NamespaceResolver.RingOffsetWidthChain);
        Assert.Equal(["--divide-width", "--border-width"], NamespaceResolver.DivideWidthChain);
        Assert.Equal(["--stroke-width"], NamespaceResolver.StrokeWidthChain);
    }

    [Fact]
    public void FilterNamespaceChains_ShouldBeCorrect()
    {
        // Test filter chains
        Assert.Equal(["--blur"], NamespaceResolver.BlurChain);
        Assert.Equal(["--brightness"], NamespaceResolver.BrightnessChain);
        Assert.Equal(["--contrast"], NamespaceResolver.ContrastChain);
        Assert.Equal(["--grayscale"], NamespaceResolver.GrayscaleChain);
        Assert.Equal(["--hue-rotate"], NamespaceResolver.HueRotateChain);
        Assert.Equal(["--invert"], NamespaceResolver.InvertChain);
        Assert.Equal(["--saturate"], NamespaceResolver.SaturateChain);
        Assert.Equal(["--sepia"], NamespaceResolver.SepiaChain);
        Assert.Equal(["--drop-shadow"], NamespaceResolver.DropShadowChain);
    }

    [Fact]
    public void BackdropFilterNamespaceChains_ShouldHaveCorrectFallbacks()
    {
        // Test backdrop filter chains
        Assert.Equal(["--backdrop-blur", "--blur"], NamespaceResolver.BackdropBlurChain);
        Assert.Equal(["--backdrop-brightness"], NamespaceResolver.BackdropBrightnessChain);
        Assert.Equal(["--backdrop-contrast"], NamespaceResolver.BackdropContrastChain);
        Assert.Equal(["--backdrop-grayscale"], NamespaceResolver.BackdropGrayscaleChain);
        Assert.Equal(["--backdrop-hue-rotate"], NamespaceResolver.BackdropHueRotateChain);
        Assert.Equal(["--backdrop-invert"], NamespaceResolver.BackdropInvertChain);
        Assert.Equal(["--backdrop-opacity"], NamespaceResolver.BackdropOpacityChain);
        Assert.Equal(["--backdrop-saturate"], NamespaceResolver.BackdropSaturateChain);
        Assert.Equal(["--backdrop-sepia"], NamespaceResolver.BackdropSepiaChain);
    }

    [Fact]
    public void TypographyNamespaceChains_ShouldBeCorrect()
    {
        // Test typography chains
        Assert.Equal(["--font-size"], NamespaceResolver.FontSizeChain);
        Assert.Equal(["--font-family"], NamespaceResolver.FontFamilyChain);
        Assert.Equal(["--font-weight"], NamespaceResolver.FontWeightChain);
        Assert.Equal(["--letter-spacing"], NamespaceResolver.LetterSpacingChain);
        Assert.Equal(["--line-height"], NamespaceResolver.LineHeightChain);
    }

    [Fact]
    public void TransformNamespaceChains_ShouldBeCorrect()
    {
        // Test transform chains
        Assert.Equal(["--scale"], NamespaceResolver.ScaleChain);
        Assert.Equal(["--rotate"], NamespaceResolver.RotateChain);
        Assert.Equal(["--skew"], NamespaceResolver.SkewChain);
        Assert.Equal(["--transform-origin"], NamespaceResolver.TransformOriginChain);
    }

    [Fact]
    public void GridNamespaceChains_ShouldBeCorrect()
    {
        // Test grid chains
        Assert.Equal(["--grid-template-columns"], NamespaceResolver.GridTemplateColumnsChain);
        Assert.Equal(["--grid-template-rows"], NamespaceResolver.GridTemplateRowsChain);
        Assert.Equal(["--grid-column"], NamespaceResolver.GridColumnChain);
        Assert.Equal(["--grid-row"], NamespaceResolver.GridRowChain);
        Assert.Equal(["--grid-auto-columns"], NamespaceResolver.GridAutoColumnsChain);
        Assert.Equal(["--grid-auto-rows"], NamespaceResolver.GridAutoRowsChain);
    }

    [Fact]
    public void BuildChain_ShouldCreateCustomNamespaceChain()
    {
        // Test custom chain builder
        var customChain = NamespaceResolver.BuildChain("--custom1", "--custom2", "--custom3");
        Assert.Equal(["--custom1", "--custom2", "--custom3"], customChain);

        // Test single namespace
        var singleChain = NamespaceResolver.BuildChain("--single");
        Assert.Equal(["--single"], singleChain);

        // Test empty chain
        var emptyChain = NamespaceResolver.BuildChain();
        Assert.Empty(emptyChain);
    }

    [Fact]
    public void AppendFallbacks_ShouldCombineNamespaceChains()
    {
        // Test appending fallbacks to existing chain
        var primary = new[] { "--primary", "--secondary" };
        var fallbacks = new[] { "--fallback1", "--fallback2" };

        var combined = NamespaceResolver.AppendFallbacks(primary, fallbacks);
        Assert.Equal(["--primary", "--secondary", "--fallback1", "--fallback2"], combined);

        // Test appending to empty primary
        var emptyPrimary = Array.Empty<string>();
        var withFallbacks = NamespaceResolver.AppendFallbacks(emptyPrimary, "--fallback");
        Assert.Equal(["--fallback"], withFallbacks);

        // Test appending empty fallbacks
        var noFallbacks = NamespaceResolver.AppendFallbacks(primary);
        Assert.Equal(primary, noFallbacks);
    }

    [Fact]
    public void EmptyNamespaceChain_ShouldBeEmpty()
    {
        // Test that Empty constant is actually empty
        Assert.Empty(NamespaceResolver.Empty);
    }

    [Fact]
    public void NamespaceConstants_ShouldFollowNamingConvention()
    {
        // Test that all namespace constants follow the -- prefix convention
        Assert.StartsWith("--", NamespaceResolver.Color);
        Assert.StartsWith("--", NamespaceResolver.Spacing);
        Assert.StartsWith("--", NamespaceResolver.BorderWidth);
        Assert.StartsWith("--", NamespaceResolver.BorderRadius);
        Assert.StartsWith("--", NamespaceResolver.Opacity);
        Assert.StartsWith("--", NamespaceResolver.FontSize);
        Assert.StartsWith("--", NamespaceResolver.FontFamily);
        Assert.StartsWith("--", NamespaceResolver.FontWeight);
        Assert.StartsWith("--", NamespaceResolver.Shadow);
        Assert.StartsWith("--", NamespaceResolver.Width);
        Assert.StartsWith("--", NamespaceResolver.Height);
        Assert.StartsWith("--", NamespaceResolver.ZIndex);
    }

    [Fact]
    public void NamespaceChains_ShouldNotContainDuplicates()
    {
        // Test that namespace chains don't have duplicates
        Assert.Equal(NamespaceResolver.BackgroundColorChain.Length,
            NamespaceResolver.BackgroundColorChain.Distinct().Count());

        Assert.Equal(NamespaceResolver.DivideColorChain.Length,
            NamespaceResolver.DivideColorChain.Distinct().Count());

        Assert.Equal(NamespaceResolver.MinWidthChain.Length,
            NamespaceResolver.MinWidthChain.Distinct().Count());

        Assert.Equal(NamespaceResolver.RingWidthChain.Length,
            NamespaceResolver.RingWidthChain.Distinct().Count());
    }

    [Fact]
    public void ComplexNamespaceChains_ShouldMaintainCorrectOrder()
    {
        // Test that complex chains maintain specific -> general order
        var divideChain = NamespaceResolver.DivideColorChain;
        Assert.Equal("--divide-color", divideChain[0]); // Most specific first
        Assert.Equal("--color", divideChain[^1]); // Most general last

        var minWidthChain = NamespaceResolver.MinWidthChain;
        Assert.Equal("--min-width", minWidthChain[0]); // Most specific first
        Assert.Contains("--spacing", minWidthChain); // Contains spacing fallback

        var ringWidthChain = NamespaceResolver.RingWidthChain;
        Assert.Equal("--ring-width", ringWidthChain[0]); // Most specific first
        Assert.Contains("--border-width", ringWidthChain); // Contains border fallback
        Assert.Contains("--spacing", ringWidthChain); // Contains spacing fallback
    }
}