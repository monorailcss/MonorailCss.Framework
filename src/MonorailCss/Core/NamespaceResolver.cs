namespace MonorailCss.Core;

/// <summary>
/// Centralized namespace resolution for MonorailCss utilities.
/// Provides standard namespace constants and pre-built namespace chains.
/// </summary>
internal static class NamespaceResolver
{
    // Core namespace constants
    public const string Color = "--color";
    public const string Spacing = "--spacing";
    public const string BorderWidth = "--border-width";
    public const string BorderRadius = "--border-radius";
    public const string Radius = "--radius";
    public const string Opacity = "--opacity";
    public const string FontSize = "--font-size";
    public const string FontFamily = "--font-family";
    public const string FontWeight = "--font-weight";
    public const string LetterSpacing = "--letter-spacing";
    public const string LineHeight = "--line-height";
    public const string Shadow = "--shadow";
    public const string Blur = "--blur";
    public const string Brightness = "--brightness";
    public const string Contrast = "--contrast";
    public const string Grayscale = "--grayscale";
    public const string HueRotate = "--hue-rotate";
    public const string Invert = "--invert";
    public const string Saturate = "--saturate";
    public const string Sepia = "--sepia";
    public const string DropShadow = "--drop-shadow";
    public const string Width = "--width";
    public const string Height = "--height";
    public const string MinWidth = "--min-width";
    public const string MinHeight = "--min-height";
    public const string MaxWidth = "--max-width";
    public const string MaxHeight = "--max-height";
    public const string FlexBasis = "--flex-basis";
    public const string Translate = "--translate";
    public const string Scale = "--scale";
    public const string Rotate = "--rotate";
    public const string Skew = "--skew";
    public const string TransformOrigin = "--transform-origin";
    public const string ZIndex = "--z-index";
    public const string Order = "--order";
    public const string GridTemplateColumns = "--grid-template-columns";
    public const string GridTemplateRows = "--grid-template-rows";
    public const string GridColumn = "--grid-column";
    public const string GridRow = "--grid-row";
    public const string GridAutoColumns = "--grid-auto-columns";
    public const string GridAutoRows = "--grid-auto-rows";
    public const string Gap = "--gap";
    public const string TransitionProperty = "--transition-property";
    public const string TransitionDuration = "--transition-duration";
    public const string TransitionTimingFunction = "--transition-timing-function";
    public const string TransitionDelay = "--transition-delay";
    public const string AnimationDuration = "--animation-duration";
    public const string AnimationDelay = "--animation-delay";
    public const string BackdropBlur = "--backdrop-blur";
    public const string BackdropBrightness = "--backdrop-brightness";
    public const string BackdropContrast = "--backdrop-contrast";
    public const string BackdropGrayscale = "--backdrop-grayscale";
    public const string BackdropHueRotate = "--backdrop-hue-rotate";
    public const string BackdropInvert = "--backdrop-invert";
    public const string BackdropOpacity = "--backdrop-opacity";
    public const string BackdropSaturate = "--backdrop-saturate";
    public const string BackdropSepia = "--backdrop-sepia";

    // Utility-specific namespaces
    public const string BackgroundColor = "--background-color";
    public const string TextColor = "--text-color";
    public const string BorderColor = "--border-color";
    public const string OutlineColor = "--outline-color";
    public const string OutlineWidth = "--outline-width";
    public const string OutlineOffset = "--outline-offset";
    public const string RingColor = "--ring-color";
    public const string RingWidth = "--ring-width";
    public const string RingOffsetColor = "--ring-offset-color";
    public const string RingOffsetWidth = "--ring-offset-width";
    public const string AccentColor = "--accent-color";
    public const string CaretColor = "--caret-color";
    public const string DivideColor = "--divide-color";
    public const string DivideWidth = "--divide-width";
    public const string FillColor = "--fill";
    public const string StrokeColor = "--stroke";
    public const string StrokeWidth = "--stroke-width";
    public const string PlaceholderColor = "--placeholder-color";
    public const string DecorationColor = "--text-decoration-color";
    public const string ShadowColor = "--shadow-color";
    public const string TextShadowColor = "--text-shadow-color";
    public const string GradientFrom = "--gradient-from";
    public const string GradientTo = "--gradient-to";
    public const string GradientVia = "--gradient-via";
    public const string Padding = "--padding";
    public const string Margin = "--margin";
    public const string Space = "--space";
    public const string ScrollMargin = "--scroll-margin";
    public const string ScrollPadding = "--scroll-padding";
    public const string TextIndent = "--text-indent";
    public const string Columns = "--columns";
    public const string AspectRatio = "--aspect-ratio";

    // Additional utility-specific namespaces
    public const string VerticalAlign = "--vertical-align";
    public const string Content = "--content";
    public const string WillChange = "--will-change";
    public const string Transition = "--transition";
    public const string Delay = "--delay";
    public const string Ease = "--ease";
    public const string Duration = "--duration";
    public const string Animate = "--animate";
    public const string FlexGrow = "--flex-grow";
    public const string FlexShrink = "--flex-shrink";
    public const string Flex = "--flex";
    public const string Text = "--text";
    public const string TextShadow = "--text-shadow";

    // Pre-built namespace arrays for common patterns
    public static readonly string[] Empty = [];

    // Color namespace chains
    public static readonly string[] BackgroundColorChain = [BackgroundColor, Color];
    public static readonly string[] TextColorChain = [TextColor, Color];
    public static readonly string[] BorderColorChain = [BorderColor, Color];
    public static readonly string[] OutlineColorChain = [OutlineColor, Color];
    public static readonly string[] RingColorChain = [RingColor, Color];
    public static readonly string[] RingOffsetColorChain = [RingOffsetColor, Color];
    public static readonly string[] AccentColorChain = [AccentColor, Color];
    public static readonly string[] CaretColorChain = [CaretColor, Color];
    public static readonly string[] DivideColorChain = [DivideColor, BorderColor, Color];
    public static readonly string[] FillColorChain = [FillColor, Color];
    public static readonly string[] StrokeColorChain = [StrokeColor, Color];
    public static readonly string[] PlaceholderColorChain = [PlaceholderColor, Color];
    public static readonly string[] DecorationColorChain = [DecorationColor, Color];
    public static readonly string[] ShadowColorChain = [ShadowColor, Color];
    public static readonly string[] TextShadowColorChain = [TextShadowColor, Color];
    public static readonly string[] GradientFromChain = [GradientFrom, Color];
    public static readonly string[] GradientToChain = [GradientTo, Color];
    public static readonly string[] GradientViaChain = [GradientVia, Color];

    // Spacing namespace chains
    public static readonly string[] PaddingChain = [Padding, Spacing];
    public static readonly string[] MarginChain = [Margin, Spacing];
    public static readonly string[] SpaceChain = [Space, Spacing];
    public static readonly string[] GapChain = [Gap, Spacing];
    public static readonly string[] ScrollMarginChain = [ScrollMargin, Spacing];
    public static readonly string[] ScrollPaddingChain = [ScrollPadding, Spacing];
    public static readonly string[] TextIndentChain = [TextIndent, Spacing];

    // Width namespace chains
    public static readonly string[] BorderWidthChain = [BorderWidth];
    public static readonly string[] OutlineWidthChain = [OutlineWidth, BorderWidth];
    public static readonly string[] OutlineOffsetChain = [OutlineOffset, Spacing];
    public static readonly string[] RingWidthChain = [RingWidth, BorderWidth, Spacing];
    public static readonly string[] RingOffsetWidthChain = [RingOffsetWidth, BorderWidth, Spacing];
    public static readonly string[] DivideWidthChain = [DivideWidth, BorderWidth];
    public static readonly string[] StrokeWidthChain = [StrokeWidth];

    // Sizing namespace chains
    public static readonly string[] WidthChain = [Width, Spacing];
    public static readonly string[] HeightChain = [Height, Spacing];
    public static readonly string[] MinWidthChain = [MinWidth, Width, Spacing];
    public static readonly string[] MinHeightChain = [MinHeight, Height, Spacing];
    public static readonly string[] MaxWidthChain = [MaxWidth, Width];
    public static readonly string[] MaxHeightChain = [MaxHeight, Height];
    public static readonly string[] FlexBasisChain = [FlexBasis, Spacing];

    // Typography namespace chains
    public static readonly string[] FontSizeChain = [FontSize];
    public static readonly string[] FontFamilyChain = [FontFamily];
    public static readonly string[] FontWeightChain = [FontWeight];
    public static readonly string[] LetterSpacingChain = [LetterSpacing];
    public static readonly string[] LineHeightChain = [LineHeight];

    // Effects namespace chains
    public static readonly string[] ShadowChain = [Shadow];
    public static readonly string[] OpacityChain = [Opacity];
    public static readonly string[] BorderRadiusChain = [BorderRadius, Radius];

    // Filter namespace chains
    public static readonly string[] BlurChain = [Blur];
    public static readonly string[] BrightnessChain = [Brightness];
    public static readonly string[] ContrastChain = [Contrast];
    public static readonly string[] GrayscaleChain = [Grayscale];
    public static readonly string[] HueRotateChain = [HueRotate];
    public static readonly string[] InvertChain = [Invert];
    public static readonly string[] SaturateChain = [Saturate];
    public static readonly string[] SepiaChain = [Sepia];
    public static readonly string[] DropShadowChain = [DropShadow];

    // Backdrop filter namespace chains
    public static readonly string[] BackdropBlurChain = [BackdropBlur, Blur];
    public static readonly string[] BackdropBrightnessChain = [BackdropBrightness];
    public static readonly string[] BackdropContrastChain = [BackdropContrast];
    public static readonly string[] BackdropGrayscaleChain = [BackdropGrayscale];
    public static readonly string[] BackdropHueRotateChain = [BackdropHueRotate];
    public static readonly string[] BackdropInvertChain = [BackdropInvert];
    public static readonly string[] BackdropOpacityChain = [BackdropOpacity];
    public static readonly string[] BackdropSaturateChain = [BackdropSaturate];
    public static readonly string[] BackdropSepiaChain = [BackdropSepia];

    // Transform namespace chains
    public static readonly string[] TranslateChain = [Translate, Spacing];
    public static readonly string[] ScaleChain = [Scale];
    public static readonly string[] RotateChain = [Rotate];
    public static readonly string[] SkewChain = [Skew];
    public static readonly string[] TransformOriginChain = [TransformOrigin];

    // Layout namespace chains
    public static readonly string[] ZIndexChain = [ZIndex];
    public static readonly string[] OrderChain = [Order];
    public static readonly string[] GridTemplateColumnsChain = [GridTemplateColumns];
    public static readonly string[] GridTemplateRowsChain = [GridTemplateRows];
    public static readonly string[] GridColumnChain = [GridColumn];
    public static readonly string[] GridRowChain = [GridRow];
    public static readonly string[] GridAutoColumnsChain = [GridAutoColumns];
    public static readonly string[] GridAutoRowsChain = [GridAutoRows];
    public static readonly string[] ColumnsChain = [Columns];
    public static readonly string[] AspectRatioChain = [AspectRatio];

    // Animation namespace chains
    public static readonly string[] TransitionPropertyChain = [TransitionProperty];
    public static readonly string[] TransitionDurationChain = [TransitionDuration];
    public static readonly string[] TransitionTimingFunctionChain = [TransitionTimingFunction];
    public static readonly string[] TransitionDelayChain = [TransitionDelay];
    public static readonly string[] AnimationDurationChain = [AnimationDuration];
    public static readonly string[] AnimationDelayChain = [AnimationDelay];

    // Additional utility namespace chains
    public static readonly string[] VerticalAlignChain = [VerticalAlign];
    public static readonly string[] ContentChain = [Content];
    public static readonly string[] WillChangeChain = [WillChange];
    public static readonly string[] TransitionChain = [Transition];
    public static readonly string[] DelayChain = [Delay];
    public static readonly string[] EaseChain = [Ease];
    public static readonly string[] DurationChain = [Duration];
    public static readonly string[] AnimateChain = [Animate];
    public static readonly string[] FlexGrowChain = [FlexGrow];
    public static readonly string[] FlexShrinkChain = [FlexShrink];
    public static readonly string[] FlexChain = [Flex];
    public static readonly string[] TextChain = [Text];
    public static readonly string[] TextShadowChain = [TextShadow];

    public static string[] BuildChain(params string[] namespaces)
    {
        return namespaces;
    }

    public static string[] AppendFallbacks(string[] primary, params string[] fallbacks)
    {
        var result = new string[primary.Length + fallbacks.Length];
        primary.CopyTo(result, 0);
        fallbacks.CopyTo(result, primary.Length);
        return result;
    }
}