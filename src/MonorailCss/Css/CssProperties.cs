using System.Diagnostics.CodeAnalysis;
#pragma warning disable CS1591
#pragma warning disable SA1600

namespace MonorailCss.Css;

/// <summary>
/// Collection of stand CSS property names.
/// </summary>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private")]
public static class CssProperties
{
    // Original properties
    public static string Appearance = nameof(Appearance).ToKebabCase();
    public static string Animation = nameof(Animation).ToKebabCase();
    public static string Background = nameof(Background).ToKebabCase();
    public static string BackgroundAttachment = nameof(BackgroundAttachment).ToKebabCase();
    public static string BackgroundClip = nameof(BackgroundClip).ToKebabCase();
    public static string BackgroundColor = nameof(BackgroundColor).ToKebabCase();
    public static string BackgroundImage = nameof(BackgroundImage).ToKebabCase();
    public static string BackgroundOrigin = nameof(BackgroundOrigin).ToKebabCase();
    public static string BackgroundPosition = nameof(BackgroundPosition).ToKebabCase();
    public static string BackgroundRepeat = nameof(BackgroundRepeat).ToKebabCase();
    public static string BackgroundSize = nameof(BackgroundSize).ToKebabCase();
    public static string Border = nameof(Border).ToKebabCase();
    public static string BorderBottom = nameof(BorderBottom).ToKebabCase();
    public static string BorderBottomColor = nameof(BorderBottomColor).ToKebabCase();
    public static string BorderBottomLeftRadius = nameof(BorderBottomLeftRadius).ToKebabCase();
    public static string BorderBottomRightRadius = nameof(BorderBottomRightRadius).ToKebabCase();
    public static string BorderBottomStyle = nameof(BorderBottomStyle).ToKebabCase();
    public static string BorderBottomWidth = nameof(BorderBottomWidth).ToKebabCase();
    public static string BorderCollapse = nameof(BorderCollapse).ToKebabCase();
    public static string BorderColor = nameof(BorderColor).ToKebabCase();
    public static string BorderImage = nameof(BorderImage).ToKebabCase();
    public static string BorderImageOutset = nameof(BorderImageOutset).ToKebabCase();
    public static string BorderImageRepeat = nameof(BorderImageRepeat).ToKebabCase();
    public static string BorderImageSlice = nameof(BorderImageSlice).ToKebabCase();
    public static string BorderImageSource = nameof(BorderImageSource).ToKebabCase();
    public static string BorderImageWidth = nameof(BorderImageWidth).ToKebabCase();
    public static string BorderLeft = nameof(BorderLeft).ToKebabCase();
    public static string BorderLeftColor = nameof(BorderLeftColor).ToKebabCase();
    public static string BorderLeftStyle = nameof(BorderLeftStyle).ToKebabCase();
    public static string BorderLeftWidth = nameof(BorderLeftWidth).ToKebabCase();
    public static string BorderRadius = nameof(BorderRadius).ToKebabCase();
    public static string BorderRight = nameof(BorderRight).ToKebabCase();
    public static string BorderRightColor = nameof(BorderRightColor).ToKebabCase();
    public static string BorderRightStyle = nameof(BorderRightStyle).ToKebabCase();
    public static string BorderRightWidth = nameof(BorderRightWidth).ToKebabCase();
    public static string BorderSpacing = nameof(BorderSpacing).ToKebabCase();
    public static string BorderStyle = nameof(BorderStyle).ToKebabCase();
    public static string BorderTop = nameof(BorderTop).ToKebabCase();
    public static string BorderTopColor = nameof(BorderTopColor).ToKebabCase();
    public static string BorderTopLeftRadius = nameof(BorderTopLeftRadius).ToKebabCase();
    public static string BorderTopRightRadius = nameof(BorderTopRightRadius).ToKebabCase();
    public static string BorderTopStyle = nameof(BorderTopStyle).ToKebabCase();
    public static string BorderTopWidth = nameof(BorderTopWidth).ToKebabCase();
    public static string BorderWidth = nameof(BorderWidth).ToKebabCase();
    public static string Bottom = nameof(Bottom).ToKebabCase();
    public static string BoxShadow = nameof(BoxShadow).ToKebabCase();
    public static string CaptionSide = nameof(CaptionSide).ToKebabCase();
    public static string Clear = nameof(Clear).ToKebabCase();
    public static string Clip = nameof(Clip).ToKebabCase();
    public static string Color = nameof(Color).ToKebabCase();
    public static string Content = nameof(Content).ToKebabCase();
    public static string CounterIncrement = nameof(CounterIncrement).ToKebabCase();
    public static string CounterReset = nameof(CounterReset).ToKebabCase();
    public static string Cursor = nameof(Cursor).ToKebabCase();
    public static string Direction = nameof(Direction).ToKebabCase();
    public static string Display = nameof(Display).ToKebabCase();
    public static string EmptyCells = nameof(EmptyCells).ToKebabCase();
    public static string Float = nameof(Float).ToKebabCase();
    public static string Font = nameof(Font).ToKebabCase();
    public static string FontFamily = nameof(FontFamily).ToKebabCase();
    public static string FontSize = nameof(FontSize).ToKebabCase();
    public static string FontSizeAdjust = nameof(FontSizeAdjust).ToKebabCase();
    public static string FontStretch = nameof(FontStretch).ToKebabCase();
    public static string FontStyle = nameof(FontStyle).ToKebabCase();
    public static string FontSynthesis = nameof(FontSynthesis).ToKebabCase();
    public static string FontVariant = nameof(FontVariant).ToKebabCase();
    public static string FontWeight = nameof(FontWeight).ToKebabCase();
    public static string Height = nameof(Height).ToKebabCase();
    public static string Left = nameof(Left).ToKebabCase();
    public static string LetterSpacing = nameof(LetterSpacing).ToKebabCase();
    public static string LineHeight = nameof(LineHeight).ToKebabCase();
    public static string ListStyle = nameof(ListStyle).ToKebabCase();
    public static string ListStyleImage = nameof(ListStyleImage).ToKebabCase();
    public static string ListStylePosition = nameof(ListStylePosition).ToKebabCase();
    public static string ListStyleType = nameof(ListStyleType).ToKebabCase();
    public static string Margin = nameof(Margin).ToKebabCase();
    public static string MarginBottom = nameof(MarginBottom).ToKebabCase();
    public static string MarginLeft = nameof(MarginLeft).ToKebabCase();
    public static string MarginRight = nameof(MarginRight).ToKebabCase();
    public static string MarginTop = nameof(MarginTop).ToKebabCase();
    public static string MaxHeight = nameof(MaxHeight).ToKebabCase();
    public static string MaxWidth = nameof(MaxWidth).ToKebabCase();
    public static string MinHeight = nameof(MinHeight).ToKebabCase();
    public static string MinWidth = nameof(MinWidth).ToKebabCase();
    public static string Opacity = nameof(Opacity).ToKebabCase();
    public static string Orphans = nameof(Orphans).ToKebabCase();
    public static string Outline = nameof(Outline).ToKebabCase();
    public static string OutlineColor = nameof(OutlineColor).ToKebabCase();
    public static string OutlineOffset = nameof(OutlineOffset).ToKebabCase();
    public static string OutlineStyle = nameof(OutlineStyle).ToKebabCase();
    public static string OutlineWidth = nameof(OutlineWidth).ToKebabCase();
    public static string Overflow = nameof(Overflow).ToKebabCase();
    public static string Padding = nameof(Padding).ToKebabCase();
    public static string PaddingBottom = nameof(PaddingBottom).ToKebabCase();
    public static string PaddingLeft = nameof(PaddingLeft).ToKebabCase();
    public static string PaddingRight = nameof(PaddingRight).ToKebabCase();
    public static string PaddingTop = nameof(PaddingTop).ToKebabCase();
    public static string PageBreakAfter = nameof(PageBreakAfter).ToKebabCase();
    public static string PageBreakBefore = nameof(PageBreakBefore).ToKebabCase();
    public static string PageBreakInside = nameof(PageBreakInside).ToKebabCase();
    public static string PointerEvents = nameof(PointerEvents).ToKebabCase();
    public static string Position = nameof(Position).ToKebabCase();
    public static string Quotes = nameof(Quotes).ToKebabCase();
    public static string Right = nameof(Right).ToKebabCase();
    public static string TableLayout = nameof(TableLayout).ToKebabCase();
    public static string TextAlign = nameof(TextAlign).ToKebabCase();
    public static string TextDecoration = nameof(TextDecoration).ToKebabCase();
    public static string TextIndent = nameof(TextIndent).ToKebabCase();
    public static string TextTransform = nameof(TextTransform).ToKebabCase();
    public static string Top = nameof(Top).ToKebabCase();
    public static string Transform = nameof(Transform).ToKebabCase();
    public static string TransformOrigin = nameof(TransformOrigin).ToKebabCase();
    public static string Transition = nameof(Transition).ToKebabCase();
    public static string TransitionDelay = nameof(TransitionDelay).ToKebabCase();
    public static string TransitionDuration = nameof(TransitionDuration).ToKebabCase();
    public static string TransitionProperty = nameof(TransitionProperty).ToKebabCase();
    public static string TransitionTimingFunction = nameof(TransitionTimingFunction).ToKebabCase();
    public static string UnicodeBidi = nameof(UnicodeBidi).ToKebabCase();
    public static string VerticalAlign = nameof(VerticalAlign).ToKebabCase();
    public static string Visibility = nameof(Visibility).ToKebabCase();
    public static string WhiteSpace = nameof(WhiteSpace).ToKebabCase();
    public static string Widows = nameof(Widows).ToKebabCase();
    public static string Width = nameof(Width).ToKebabCase();
    public static string WordSpacing = nameof(WordSpacing).ToKebabCase();
    public static string ZIndex = nameof(ZIndex).ToKebabCase();

    // Flexbox properties
    public static string AlignContent = nameof(AlignContent).ToKebabCase();
    public static string AlignItems = nameof(AlignItems).ToKebabCase();
    public static string AlignSelf = nameof(AlignSelf).ToKebabCase();
    public static string Flex = nameof(Flex).ToKebabCase();
    public static string FlexBasis = nameof(FlexBasis).ToKebabCase();
    public static string FlexDirection = nameof(FlexDirection).ToKebabCase();
    public static string FlexFlow = nameof(FlexFlow).ToKebabCase();
    public static string FlexGrow = nameof(FlexGrow).ToKebabCase();
    public static string FlexShrink = nameof(FlexShrink).ToKebabCase();
    public static string FlexWrap = nameof(FlexWrap).ToKebabCase();
    public static string JustifyContent = nameof(JustifyContent).ToKebabCase();
    public static string Order = nameof(Order).ToKebabCase();

    // Grid properties
    public static string Grid = nameof(Grid).ToKebabCase();
    public static string GridArea = nameof(GridArea).ToKebabCase();
    public static string GridAutoColumns = nameof(GridAutoColumns).ToKebabCase();
    public static string GridAutoFlow = nameof(GridAutoFlow).ToKebabCase();
    public static string GridAutoRows = nameof(GridAutoRows).ToKebabCase();
    public static string GridColumn = nameof(GridColumn).ToKebabCase();
    public static string GridColumnEnd = nameof(GridColumnEnd).ToKebabCase();
    public static string GridColumnGap = nameof(GridColumnGap).ToKebabCase();
    public static string GridColumnStart = nameof(GridColumnStart).ToKebabCase();
    public static string GridGap = nameof(GridGap).ToKebabCase();
    public static string GridRow = nameof(GridRow).ToKebabCase();
    public static string GridRowEnd = nameof(GridRowEnd).ToKebabCase();
    public static string GridRowGap = nameof(GridRowGap).ToKebabCase();
    public static string GridRowStart = nameof(GridRowStart).ToKebabCase();
    public static string GridTemplate = nameof(GridTemplate).ToKebabCase();
    public static string GridTemplateAreas = nameof(GridTemplateAreas).ToKebabCase();
    public static string GridTemplateColumns = nameof(GridTemplateColumns).ToKebabCase();
    public static string GridTemplateRows = nameof(GridTemplateRows).ToKebabCase();

    // Gap properties (replacing old Grid-specific gap properties)
    public static string Gap = nameof(Gap).ToKebabCase();
    public static string ColumnGap = nameof(ColumnGap).ToKebabCase();
    public static string RowGap = nameof(RowGap).ToKebabCase();

    // Text properties
    public static string TextDecorationColor = nameof(TextDecorationColor).ToKebabCase();
    public static string TextDecorationLine = nameof(TextDecorationLine).ToKebabCase();
    public static string TextDecorationStyle = nameof(TextDecorationStyle).ToKebabCase();
    public static string TextDecorationThickness = nameof(TextDecorationThickness).ToKebabCase();
    public static string TextUnderlineOffset = nameof(TextUnderlineOffset).ToKebabCase();
    public static string TextShadow = nameof(TextShadow).ToKebabCase();
    public static string TextOverflow = nameof(TextOverflow).ToKebabCase();
    public static string TextRendering = nameof(TextRendering).ToKebabCase();
    public static string TextAlignLast = nameof(TextAlignLast).ToKebabCase();
    public static string TextJustify = nameof(TextJustify).ToKebabCase();
    public static string TextOrientation = nameof(TextOrientation).ToKebabCase();
    public static string TextCombineUpright = nameof(TextCombineUpright).ToKebabCase();

    // Font properties
    public static string FontDisplay = nameof(FontDisplay).ToKebabCase();
    public static string FontVariantCaps = nameof(FontVariantCaps).ToKebabCase();
    public static string FontVariantEastAsian = nameof(FontVariantEastAsian).ToKebabCase();
    public static string FontVariantLigatures = nameof(FontVariantLigatures).ToKebabCase();
    public static string FontVariantNumeric = nameof(FontVariantNumeric).ToKebabCase();
    public static string FontFeatureSettings = nameof(FontFeatureSettings).ToKebabCase();
    public static string FontLanguageOverride = nameof(FontLanguageOverride).ToKebabCase();
    public static string FontVariationSettings = nameof(FontVariationSettings).ToKebabCase();

    // Animation and transition properties
    public static string AnimationDelay = nameof(AnimationDelay).ToKebabCase();
    public static string AnimationDirection = nameof(AnimationDirection).ToKebabCase();
    public static string AnimationDuration = nameof(AnimationDuration).ToKebabCase();
    public static string AnimationFillMode = nameof(AnimationFillMode).ToKebabCase();
    public static string AnimationIterationCount = nameof(AnimationIterationCount).ToKebabCase();
    public static string AnimationName = nameof(AnimationName).ToKebabCase();
    public static string AnimationPlayState = nameof(AnimationPlayState).ToKebabCase();
    public static string AnimationTimingFunction = nameof(AnimationTimingFunction).ToKebabCase();
    public static string BackfaceVisibility = nameof(BackfaceVisibility).ToKebabCase();
    public static string TransformStyle = nameof(TransformStyle).ToKebabCase();
    public static string Perspective = nameof(Perspective).ToKebabCase();
    public static string PerspectiveOrigin = nameof(PerspectiveOrigin).ToKebabCase();

    // Box model properties
    public static string BoxSizing = nameof(BoxSizing).ToKebabCase();
    public static string OverflowX = nameof(OverflowX).ToKebabCase();
    public static string OverflowY = nameof(OverflowY).ToKebabCase();
    public static string OverflowWrap = nameof(OverflowWrap).ToKebabCase();
    public static string OverflowAnchor = nameof(OverflowAnchor).ToKebabCase();
    public static string Resize = nameof(Resize).ToKebabCase();

    // Logical properties
    public static string BlockSize = nameof(BlockSize).ToKebabCase();
    public static string InlineSize = nameof(InlineSize).ToKebabCase();
    public static string MinBlockSize = nameof(MinBlockSize).ToKebabCase();
    public static string MinInlineSize = nameof(MinInlineSize).ToKebabCase();
    public static string MaxBlockSize = nameof(MaxBlockSize).ToKebabCase();
    public static string MaxInlineSize = nameof(MaxInlineSize).ToKebabCase();
    public static string MarginBlock = nameof(MarginBlock).ToKebabCase();
    public static string MarginBlockEnd = nameof(MarginBlockEnd).ToKebabCase();
    public static string MarginBlockStart = nameof(MarginBlockStart).ToKebabCase();
    public static string MarginInline = nameof(MarginInline).ToKebabCase();
    public static string MarginInlineEnd = nameof(MarginInlineEnd).ToKebabCase();
    public static string MarginInlineStart = nameof(MarginInlineStart).ToKebabCase();
    public static string PaddingBlock = nameof(PaddingBlock).ToKebabCase();
    public static string PaddingBlockEnd = nameof(PaddingBlockEnd).ToKebabCase();
    public static string PaddingBlockStart = nameof(PaddingBlockStart).ToKebabCase();
    public static string PaddingInline = nameof(PaddingInline).ToKebabCase();
    public static string PaddingInlineEnd = nameof(PaddingInlineEnd).ToKebabCase();
    public static string PaddingInlineStart = nameof(PaddingInlineStart).ToKebabCase();
    public static string BorderBlock = nameof(BorderBlock).ToKebabCase();
    public static string BorderBlockEnd = nameof(BorderBlockEnd).ToKebabCase();
    public static string BorderBlockStart = nameof(BorderBlockStart).ToKebabCase();
    public static string BorderInline = nameof(BorderInline).ToKebabCase();
    public static string BorderInlineEnd = nameof(BorderInlineEnd).ToKebabCase();
    public static string BorderInlineStart = nameof(BorderInlineStart).ToKebabCase();
    public static string InsetBlockEnd = nameof(InsetBlockEnd).ToKebabCase();
    public static string InsetBlockStart = nameof(InsetBlockStart).ToKebabCase();
    public static string InsetInlineEnd = nameof(InsetInlineEnd).ToKebabCase();
    public static string InsetInlineStart = nameof(InsetInlineStart).ToKebabCase();

    // Scroll properties
    public static string ScrollBehavior = nameof(ScrollBehavior).ToKebabCase();
    public static string ScrollSnapType = nameof(ScrollSnapType).ToKebabCase();
    public static string ScrollSnapAlign = nameof(ScrollSnapAlign).ToKebabCase();
    public static string ScrollPadding = nameof(ScrollPadding).ToKebabCase();
    public static string ScrollMargin = nameof(ScrollMargin).ToKebabCase();
    public static string ScrollbarWidth = nameof(ScrollbarWidth).ToKebabCase();
    public static string ScrollbarColor = nameof(ScrollbarColor).ToKebabCase();

    // Other modern properties
    public static string ObjectFit = nameof(ObjectFit).ToKebabCase();
    public static string ObjectPosition = nameof(ObjectPosition).ToKebabCase();
    public static string Isolation = nameof(Isolation).ToKebabCase();
    public static string MixBlendMode = nameof(MixBlendMode).ToKebabCase();
    public static string BackgroundBlendMode = nameof(BackgroundBlendMode).ToKebabCase();
    public static string ClipPath = nameof(ClipPath).ToKebabCase();
    public static string Mask = nameof(Mask).ToKebabCase();
    public static string MaskImage = nameof(MaskImage).ToKebabCase();
    public static string MaskPosition = nameof(MaskPosition).ToKebabCase();
    public static string MaskSize = nameof(MaskSize).ToKebabCase();
    public static string MaskRepeat = nameof(MaskRepeat).ToKebabCase();
    public static string MaskOrigin = nameof(MaskOrigin).ToKebabCase();
    public static string MaskClip = nameof(MaskClip).ToKebabCase();
    public static string MaskComposite = nameof(MaskComposite).ToKebabCase();
    public static string MaskBorderSource = nameof(MaskBorderSource).ToKebabCase();
    public static string MaskBorderMode = nameof(MaskBorderMode).ToKebabCase();
    public static string MaskBorderSlice = nameof(MaskBorderSlice).ToKebabCase();
    public static string MaskBorderWidth = nameof(MaskBorderWidth).ToKebabCase();
    public static string MaskBorderOutset = nameof(MaskBorderOutset).ToKebabCase();
    public static string MaskBorderRepeat = nameof(MaskBorderRepeat).ToKebabCase();
    public static string MaskType = nameof(MaskType).ToKebabCase();
    public static string ShapeOutside = nameof(ShapeOutside).ToKebabCase();
    public static string ShapeMargin = nameof(ShapeMargin).ToKebabCase();
    public static string ShapeImageThreshold = nameof(ShapeImageThreshold).ToKebabCase();
    public static string Filter = nameof(Filter).ToKebabCase();
    public static string Backdrop = nameof(Backdrop).ToKebabCase();
    public static string BackdropFilter = nameof(BackdropFilter).ToKebabCase();
    public static string Hyphens = nameof(Hyphens).ToKebabCase();
    public static string LineBreak = nameof(LineBreak).ToKebabCase();
    public static string WordBreak = nameof(WordBreak).ToKebabCase();
    public static string WordWrap = nameof(WordWrap).ToKebabCase();
    public static string WritingMode = nameof(WritingMode).ToKebabCase();
    public static string Contain = nameof(Contain).ToKebabCase();
    public static string ContainIntrinsicSize = nameof(ContainIntrinsicSize).ToKebabCase();
    public static string ContentVisibility = nameof(ContentVisibility).ToKebabCase();
    public static string UserSelect = nameof(UserSelect).ToKebabCase();
    public static string TouchAction = nameof(TouchAction).ToKebabCase();
    public static string WillChange = nameof(WillChange).ToKebabCase();
    public static string Zoom = nameof(Zoom).ToKebabCase();

    // CSS Variables
    public static string Variables = "--";

    // Color properties
    public static string ColorAdjust = nameof(ColorAdjust).ToKebabCase();
    public static string ColorInterpolation = nameof(ColorInterpolation).ToKebabCase();
    public static string ColorRendering = nameof(ColorRendering).ToKebabCase();
    public static string ColorScheme = nameof(ColorScheme).ToKebabCase();
    public static string AccentColor = nameof(AccentColor).ToKebabCase();

    // Container queries
    public static string Container = nameof(Container).ToKebabCase();
    public static string ContainerType = nameof(ContainerType).ToKebabCase();
    public static string ContainerName = nameof(ContainerName).ToKebabCase();

    // Aspect ratio
    public static string AspectRatio = nameof(AspectRatio).ToKebabCase();

    // Columns
    public static string Columns = nameof(Columns).ToKebabCase();
    public static string ColumnCount = nameof(ColumnCount).ToKebabCase();
    public static string ColumnFill = nameof(ColumnFill).ToKebabCase();
    public static string ColumnRule = nameof(ColumnRule).ToKebabCase();
    public static string ColumnRuleColor = nameof(ColumnRuleColor).ToKebabCase();
    public static string ColumnRuleStyle = nameof(ColumnRuleStyle).ToKebabCase();
    public static string ColumnRuleWidth = nameof(ColumnRuleWidth).ToKebabCase();
    public static string ColumnSpan = nameof(ColumnSpan).ToKebabCase();
    public static string ColumnWidth = nameof(ColumnWidth).ToKebabCase();

    // Print properties
    public static string BreakAfter = nameof(BreakAfter).ToKebabCase();
    public static string BreakBefore = nameof(BreakBefore).ToKebabCase();
    public static string BreakInside = nameof(BreakInside).ToKebabCase();

    // New positioning properties
    public static string Inset = nameof(Inset).ToKebabCase();
    public static string InsetBlock = nameof(InsetBlock).ToKebabCase();
    public static string InsetInline = nameof(InsetInline).ToKebabCase();
}