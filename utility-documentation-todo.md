# Utility Documentation Standardization

## Task Description

We are standardizing the XML documentation summaries for all utility classes in MonorailCSS. Each utility's `<summary>` tag should provide a helpful, approachable, and kind description of what the utility does.

The documentation should follow this pattern:
- "Utilities for controlling [what the utility controls/does]"
- Examples:
  - "Utilities for controlling how an element handles content that is too large for the container"
  - "Utilities for controlling the wrapping of content around an element"
  - "Utilities for controlling the color of text content"

## Utilities to Update

### Accessibility
- [x] ForcedColorAdjustUtility.cs
- [x] ScreenReaderUtility.cs

### Backgrounds
- [x] BackgroundAttachmentUtility.cs
- [x] BackgroundClipUtility.cs
- [x] BackgroundColorUtility.cs
- [x] BackgroundImageUtility.cs
- [x] BackgroundOriginUtility.cs
- [x] BackgroundPositionUtility.cs
- [x] BackgroundRepeatUtility.cs
- [x] BackgroundSizeUtility.cs
- [x] GradientFromPositionUtility.cs
- [x] GradientFromUtility.cs
- [x] GradientToPositionUtility.cs
- [x] GradientToUtility.cs
- [x] GradientViaNoneUtility.cs
- [x] GradientViaPositionUtility.cs
- [x] GradientViaUtility.cs

### Borders
- [x] BorderImageUtility.cs
- [x] BorderRadiusUtility.cs
- [x] BorderStyleUtility.cs
- [x] BorderUtility.cs
- [x] DivideColorUtility.cs
- [x] DivideStyleUtility.cs
- [x] DivideUtility.cs
- [x] DivideXReverseUtility.cs
- [x] DivideYReverseUtility.cs
- [x] InsetRingUtility.cs
- [x] OutlineColorUtility.cs
- [x] OutlineHiddenUtility.cs
- [x] OutlineOffsetUtility.cs
- [x] OutlineStyleUtility.cs
- [x] OutlineUtility.cs
- [x] OutlineWidthUtility.cs
- [x] RingInsetUtility.cs
- [x] RingOffsetColorUtility.cs
- [x] RingOffsetWidthUtility.cs
- [x] RingUtility.cs

### Container
- [x] ContainerUtility.cs

### Effects
- [x] BackgroundBlendModeUtility.cs
- [x] BoxShadowUtility.cs
- [x] DropShadowColorUtility.cs
- [x] InsetShadowColorUtility.cs
- [x] InsetShadowUtility.cs
- [x] MaskUtility.cs
- [x] MixBlendModeUtility.cs
- [x] OpacityUtility.cs
- [x] ShadowColorStaticUtility.cs
- [x] ShadowColorUtility.cs
- [x] TextShadowColorStaticUtility.cs
- [x] TextShadowColorUtility.cs
- [x] TextShadowUtility.cs

### Filters
- [x] BackdropBlurUtility.cs
- [x] BackdropBrightnessUtility.cs
- [x] BackdropContrastUtility.cs
- [x] BackdropGrayscaleUtility.cs
- [x] BackdropHueRotateUtility.cs
- [x] BackdropInvertUtility.cs
- [x] BackdropOpacityUtility.cs
- [x] BackdropSaturateUtility.cs
- [x] BackdropSepiaUtility.cs
- [x] BlurUtility.cs
- [x] BrightnessUtility.cs
- [x] ContrastUtility.cs
- [x] DropShadowUtility.cs
- [x] GrayscaleUtility.cs
- [x] HueRotateUtility.cs
- [x] InvertUtility.cs
- [x] SaturateUtility.cs
- [x] SepiaUtility.cs

### Flexbox & Grid
- [x] AlignContentUtility.cs
- [x] AlignItemsUtility.cs
- [x] AlignSelfUtility.cs
- [x] FlexBasisUtility.cs
- [x] FlexDirectionUtility.cs
- [x] FlexGrowUtility.cs
- [x] FlexShrinkUtility.cs
- [x] FlexUtility.cs
- [x] FlexWrapUtility.cs
- [x] GapUtility.cs
- [x] GridAutoColumnsUtility.cs
- [x] GridAutoRowsUtility.cs
- [x] GridColumnAutoUtility.cs
- [x] GridColumnEndUtility.cs
- [x] GridColumnSpanUtility.cs
- [x] GridColumnStartUtility.cs
- [x] GridFlowUtility.cs
- [x] GridRowAutoUtility.cs
- [x] GridRowEndUtility.cs
- [x] GridRowSpanUtility.cs
- [x] GridRowStartUtility.cs
- [x] GridTemplateColumnsUtility.cs
- [x] GridTemplateRowsUtility.cs
- [x] JustifyContentUtility.cs
- [x] JustifyItemsUtility.cs
- [x] JustifySelfUtility.cs
- [x] OrderStaticUtility.cs
- [x] OrderUtility.cs
- [x] PlaceUtility.cs

### Interactivity
- [x] AccentColorUtility.cs
- [x] AppearanceUtility.cs
- [x] CaretColorUtility.cs
- [x] ColorSchemeUtility.cs
- [x] CursorUtility.cs
- [x] FieldSizingUtility.cs
- [x] PointerEventsUtility.cs
- [x] ResizeUtility.cs
- [x] ScrollBehaviorUtility.cs
- [x] ScrollMarginUtility.cs
- [x] ScrollPaddingUtility.cs
- [x] ScrollSnapAlignUtility.cs
- [x] ScrollSnapStopUtility.cs
- [x] ScrollSnapStrictnessUtility.cs
- [x] ScrollSnapTypeUtility.cs
- [x] TouchActionUtility.cs
- [x] UserSelectUtility.cs

### Layout
- [x] AspectRatioUtility.cs
- [x] BoxDecorationBreakUtility.cs
- [x] BoxSizingUtility.cs
- [x] BreakUtility.cs
- [x] ClearUtility.cs
- [x] ColumnsUtility.cs
- [x] ContainerQueryUtility.cs
- [x] ContainUtility.cs
- [x] DisplayUtility.cs
- [x] FloatUtility.cs
- [x] InsetUtility.cs
- [x] InsetXUtility.cs
- [x] InsetYUtility.cs
- [x] IsolationUtility.cs
- [x] ObjectFitUtility.cs
- [x] ObjectPositionUtility.cs
- [x] ObjectViewBoxUtility.cs
- [x] OverflowUtility.cs
- [x] OverscrollBehaviorUtility.cs
- [x] PositionUtility.cs
- [x] VisibilityUtility.cs
- [x] ZIndexUtility.cs

### Sizing
- [x] HeightUtility.cs
- [x] MaxHeightUtility.cs
- [x] MaxWidthUtility.cs
- [x] MinHeightUtility.cs
- [x] MinWidthUtility.cs
- [x] SizeUtility.cs
- [x] WidthUtility.cs

### Spacing
- [x] MarginUtility.cs
- [x] PaddingUtility.cs
- [x] SpaceXReverseUtility.cs
- [x] SpaceXUtility.cs
- [x] SpaceYReverseUtility.cs
- [x] SpaceYUtility.cs

### SVG
- [x] FillUtility.cs
- [x] StrokeUtility.cs

### Tables
- [x] BorderCollapseUtility.cs
- [x] BorderSpacingUtility.cs
- [x] CaptionSideUtility.cs
- [x] TableLayoutUtility.cs

### Transforms
- [x] BackfaceVisibilityUtility.cs
- [x] PerspectiveFunctionalUtility.cs
- [x] PerspectiveOriginUtility.cs
- [x] PerspectiveStaticUtility.cs
- [x] RotateUtility.cs
- [x] ScaleUtility.cs
- [x] SkewUtility.cs
- [x] TransformBoxUtility.cs
- [x] TransformOriginUtility.cs
- [x] TransformStyleUtility.cs
- [x] TransformUtility.cs
- [x] TranslateUtility.cs

### Transitions & Animation
- [x] AnimationUtility.cs
- [x] TransitionBehaviorUtility.cs
- [x] TransitionDelayUtility.cs
- [x] TransitionDurationUtility.cs
- [x] TransitionTimingFunctionUtility.cs
- [x] TransitionUtility.cs
- [x] WillChangeUtility.cs

### Typography
- [x] BoxDecorationBreakUtility.cs
- [x] ContentUtility.cs
- [x] FontFamilyUtility.cs
- [x] FontSmoothingUtility.cs
- [x] FontStretchUtility.cs
- [x] FontStyleUtility.cs
- [x] FontVariantNumericUtility.cs
- [x] FontWeightUtility.cs
- [x] HyphensUtility.cs
- [x] LetterSpacingUtility.cs
- [x] LineClampUtility.cs
- [x] LineHeightUtility.cs
- [x] ListStyleImageUtility.cs
- [x] ListStylePositionUtility.cs
- [x] ListStyleTypeUtility.cs
- [x] OverflowWrapUtility.cs
- [x] ProseBaseUtility.cs
- [x] ProseColorUtility.cs
- [x] ProseInvertUtility.cs
- [x] ProseSizeUtility.cs
- [x] TextAlignUtility.cs
- [x] TextDecorationColorUtility.cs
- [x] TextDecorationStyleUtility.cs
- [x] TextDecorationThicknessUtility.cs
- [x] TextDecorationUtility.cs
- [x] TextIndentUtility.cs
- [x] TextOverflowUtility.cs
- [x] TextSizeAdjustUtility.cs
- [x] TextTransformUtility.cs
- [x] TextUnderlineOffsetUtility.cs
- [x] TextUtility.cs
- [x] TextWrapUtility.cs
- [x] VerticalAlignUtility.cs
- [x] WhitespaceUtility.cs
- [x] WordBreakUtility.cs

---

**Total Utilities:** 225

## Notes

- Each utility should have a concise, user-friendly description
- Use consistent language patterns across similar utilities
- Focus on what developers can accomplish with the utility, not implementation details
- Keep the tone helpful and approachable
