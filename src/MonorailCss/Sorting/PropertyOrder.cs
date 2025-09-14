namespace MonorailCss.Sorting;

internal static class PropertyOrder
{
    private static readonly Dictionary<string, int> _propertyOrderMap;

    static PropertyOrder()
    {
        _propertyOrderMap = BuildPropertyOrderMap();
    }

    private static int GetOrder(string property)
    {
        return _propertyOrderMap.GetValueOrDefault(property, int.MaxValue);
    }

    private static int[] GetOrders(IEnumerable<string> properties)
    {
        return properties.Select(GetOrder).ToArray();
    }

    public static int GetMinOrder(IEnumerable<string> properties)
    {
        var orders = GetOrders(properties);
        return orders.Length > 0 ? orders.Min() : int.MaxValue;
    }

    private static Dictionary<string, int> BuildPropertyOrderMap()
    {
        var order = 0;
        var map = new Dictionary<string, int>();

        // Container
        map["container-type"] = order++;

        // Pointer Events & Visibility
        map["pointer-events"] = order++;
        map["visibility"] = order++;
        map["position"] = order++;

        // Inset properties
        map["inset"] = order++;
        map["inset-inline"] = order++;
        map["inset-block"] = order++;
        map["inset-inline-start"] = order++;
        map["inset-inline-end"] = order++;
        map["top"] = order++;
        map["right"] = order++;
        map["bottom"] = order++;
        map["left"] = order++;

        // Z-index and stacking
        map["isolation"] = order++;
        map["z-index"] = order++;
        map["order"] = order++;

        // Grid positioning
        map["grid-column"] = order++;
        map["grid-column-start"] = order++;
        map["grid-column-end"] = order++;
        map["grid-row"] = order++;
        map["grid-row-start"] = order++;
        map["grid-row-end"] = order++;

        // Float and clear
        map["float"] = order++;
        map["clear"] = order++;

        // Container component
        map["--tw-container-component"] = order++;

        // Margin
        map["margin"] = order++;
        map["margin-inline"] = order++;
        map["margin-block"] = order++;
        map["margin-inline-start"] = order++;
        map["margin-inline-end"] = order++;
        map["margin-top"] = order++;
        map["margin-right"] = order++;
        map["margin-bottom"] = order++;
        map["margin-left"] = order++;

        // Box model
        map["box-sizing"] = order++;
        map["display"] = order++;

        // Field sizing
        map["field-sizing"] = order++;
        map["aspect-ratio"] = order++;

        // Height
        map["height"] = order++;
        map["max-height"] = order++;
        map["min-height"] = order++;

        // Width
        map["width"] = order++;
        map["max-width"] = order++;
        map["min-width"] = order++;

        // Flexbox properties
        map["flex"] = order++;
        map["flex-shrink"] = order++;
        map["flex-grow"] = order++;
        map["flex-basis"] = order++;

        // Table
        map["table-layout"] = order++;
        map["caption-side"] = order++;
        map["border-collapse"] = order++;

        // Border spacing
        map["border-spacing"] = order++;
        map["--tw-border-spacing-x"] = order++;
        map["--tw-border-spacing-y"] = order++;

        // Transform
        map["transform-origin"] = order++;

        // Translate
        map["translate"] = order++;
        map["--tw-translate-x"] = order++;
        map["--tw-translate-y"] = order++;
        map["--tw-translate-z"] = order++;

        // Scale
        map["scale"] = order++;
        map["--tw-scale-x"] = order++;
        map["--tw-scale-y"] = order++;
        map["--tw-scale-z"] = order++;

        // Rotate
        map["rotate"] = order++;
        map["--tw-rotate-x"] = order++;
        map["--tw-rotate-y"] = order++;
        map["--tw-rotate-z"] = order++;

        // Skew
        map["--tw-skew-x"] = order++;
        map["--tw-skew-y"] = order++;
        map["transform"] = order++;

        // Animation
        map["animation"] = order++;

        // Cursor
        map["cursor"] = order++;

        // Touch
        map["touch-action"] = order++;
        map["--tw-pan-x"] = order++;
        map["--tw-pan-y"] = order++;
        map["--tw-pinch-zoom"] = order++;

        // Resize
        map["resize"] = order++;

        // Scroll snap
        map["scroll-snap-type"] = order++;
        map["--tw-scroll-snap-strictness"] = order++;
        map["scroll-snap-align"] = order++;
        map["scroll-snap-stop"] = order++;

        // Scroll margin
        map["scroll-margin"] = order++;
        map["scroll-margin-inline"] = order++;
        map["scroll-margin-block"] = order++;
        map["scroll-margin-inline-start"] = order++;
        map["scroll-margin-inline-end"] = order++;
        map["scroll-margin-top"] = order++;
        map["scroll-margin-right"] = order++;
        map["scroll-margin-bottom"] = order++;
        map["scroll-margin-left"] = order++;

        // Scroll padding
        map["scroll-padding"] = order++;
        map["scroll-padding-inline"] = order++;
        map["scroll-padding-block"] = order++;
        map["scroll-padding-inline-start"] = order++;
        map["scroll-padding-inline-end"] = order++;
        map["scroll-padding-top"] = order++;
        map["scroll-padding-right"] = order++;
        map["scroll-padding-bottom"] = order++;
        map["scroll-padding-left"] = order++;

        // List
        map["list-style-position"] = order++;
        map["list-style-type"] = order++;
        map["list-style-image"] = order++;

        // Appearance
        map["appearance"] = order++;

        // Columns
        map["columns"] = order++;
        map["break-before"] = order++;
        map["break-inside"] = order++;
        map["break-after"] = order++;

        // Grid
        map["grid-auto-columns"] = order++;
        map["grid-auto-flow"] = order++;
        map["grid-auto-rows"] = order++;
        map["grid-template-columns"] = order++;
        map["grid-template-rows"] = order++;

        // Flex layout
        map["flex-direction"] = order++;
        map["flex-wrap"] = order++;
        map["place-content"] = order++;
        map["place-items"] = order++;
        map["align-content"] = order++;
        map["align-items"] = order++;
        map["justify-content"] = order++;
        map["justify-items"] = order++;
        map["gap"] = order++;
        map["column-gap"] = order++;
        map["row-gap"] = order++;

        // Space
        map["--tw-space-x-reverse"] = order++;
        map["--tw-space-y-reverse"] = order++;

        // Divide
        map["divide-x-width"] = order++;
        map["divide-y-width"] = order++;
        map["--tw-divide-y-reverse"] = order++;
        map["divide-style"] = order++;
        map["divide-color"] = order++;

        // Place
        map["place-self"] = order++;
        map["align-self"] = order++;
        map["justify-self"] = order++;

        // Overflow
        map["overflow"] = order++;
        map["overflow-x"] = order++;
        map["overflow-y"] = order++;

        // Overscroll
        map["overscroll-behavior"] = order++;
        map["overscroll-behavior-x"] = order++;
        map["overscroll-behavior-y"] = order++;

        // Scroll
        map["scroll-behavior"] = order++;

        // Border radius
        map["border-radius"] = order++;
        map["border-start-radius"] = order++;
        map["border-end-radius"] = order++;
        map["border-top-radius"] = order++;
        map["border-right-radius"] = order++;
        map["border-bottom-radius"] = order++;
        map["border-left-radius"] = order++;
        map["border-start-start-radius"] = order++;
        map["border-start-end-radius"] = order++;
        map["border-end-end-radius"] = order++;
        map["border-end-start-radius"] = order++;
        map["border-top-left-radius"] = order++;
        map["border-top-right-radius"] = order++;
        map["border-bottom-right-radius"] = order++;
        map["border-bottom-left-radius"] = order++;

        // Border width
        map["border-width"] = order++;
        map["border-inline-width"] = order++;
        map["border-block-width"] = order++;
        map["border-inline-start-width"] = order++;
        map["border-inline-end-width"] = order++;
        map["border-top-width"] = order++;
        map["border-right-width"] = order++;
        map["border-bottom-width"] = order++;
        map["border-left-width"] = order++;

        // Border style
        map["border-style"] = order++;
        map["border-inline-style"] = order++;
        map["border-block-style"] = order++;
        map["border-inline-start-style"] = order++;
        map["border-inline-end-style"] = order++;
        map["border-top-style"] = order++;
        map["border-right-style"] = order++;
        map["border-bottom-style"] = order++;
        map["border-left-style"] = order++;

        // Border color
        map["border-color"] = order++;
        map["border-inline-color"] = order++;
        map["border-block-color"] = order++;
        map["border-inline-start-color"] = order++;
        map["border-inline-end-color"] = order++;
        map["border-top-color"] = order++;
        map["border-right-color"] = order++;
        map["border-bottom-color"] = order++;
        map["border-left-color"] = order++;

        // Background color (THIS IS KEY - comes before padding!)
        map["background-color"] = order++;

        // Background image and gradients
        map["background-image"] = order++;
        map["--tw-gradient-position"] = order++;
        map["--tw-gradient-stops"] = order++;
        map["--tw-gradient-via-stops"] = order++;
        map["--tw-gradient-from"] = order++;
        map["--tw-gradient-from-position"] = order++;
        map["--tw-gradient-via"] = order++;
        map["--tw-gradient-via-position"] = order++;
        map["--tw-gradient-to"] = order++;
        map["--tw-gradient-to-position"] = order++;

        // Mask image
        map["mask-image"] = order++;

        // Edge masks
        map["--tw-mask-top"] = order++;
        map["--tw-mask-top-from-color"] = order++;
        map["--tw-mask-top-from-position"] = order++;
        map["--tw-mask-top-to-color"] = order++;
        map["--tw-mask-top-to-position"] = order++;

        map["--tw-mask-right"] = order++;
        map["--tw-mask-right-from-color"] = order++;
        map["--tw-mask-right-from-position"] = order++;
        map["--tw-mask-right-to-color"] = order++;
        map["--tw-mask-right-to-position"] = order++;

        map["--tw-mask-bottom"] = order++;
        map["--tw-mask-bottom-from-color"] = order++;
        map["--tw-mask-bottom-from-position"] = order++;
        map["--tw-mask-bottom-to-color"] = order++;
        map["--tw-mask-bottom-to-position"] = order++;

        map["--tw-mask-left"] = order++;
        map["--tw-mask-left-from-color"] = order++;
        map["--tw-mask-left-from-position"] = order++;
        map["--tw-mask-left-to-color"] = order++;
        map["--tw-mask-left-to-position"] = order++;

        // Linear masks
        map["--tw-mask-linear"] = order++;
        map["--tw-mask-linear-position"] = order++;
        map["--tw-mask-linear-from-color"] = order++;
        map["--tw-mask-linear-from-position"] = order++;
        map["--tw-mask-linear-to-color"] = order++;
        map["--tw-mask-linear-to-position"] = order++;

        // Radial masks
        map["--tw-mask-radial"] = order++;
        map["--tw-mask-radial-shape"] = order++;
        map["--tw-mask-radial-size"] = order++;
        map["--tw-mask-radial-position"] = order++;
        map["--tw-mask-radial-from-color"] = order++;
        map["--tw-mask-radial-from-position"] = order++;
        map["--tw-mask-radial-to-color"] = order++;
        map["--tw-mask-radial-to-position"] = order++;

        // Conic masks
        map["--tw-mask-conic"] = order++;
        map["--tw-mask-conic-position"] = order++;
        map["--tw-mask-conic-from-color"] = order++;
        map["--tw-mask-conic-from-position"] = order++;
        map["--tw-mask-conic-to-color"] = order++;
        map["--tw-mask-conic-to-position"] = order++;

        // Box decoration
        map["box-decoration-break"] = order++;

        // Background properties
        map["background-size"] = order++;
        map["background-attachment"] = order++;
        map["background-clip"] = order++;
        map["background-position"] = order++;
        map["background-repeat"] = order++;
        map["background-origin"] = order++;

        // Mask properties
        map["mask-composite"] = order++;
        map["mask-mode"] = order++;
        map["mask-type"] = order++;
        map["mask-size"] = order++;
        map["mask-clip"] = order++;
        map["mask-position"] = order++;
        map["mask-repeat"] = order++;
        map["mask-origin"] = order++;

        // SVG
        map["fill"] = order++;
        map["stroke"] = order++;
        map["stroke-width"] = order++;

        // Object
        map["object-fit"] = order++;
        map["object-position"] = order++;

        // Padding (comes AFTER background-color!)
        map["padding"] = order++;
        map["padding-inline"] = order++;
        map["padding-block"] = order++;
        map["padding-inline-start"] = order++;
        map["padding-inline-end"] = order++;
        map["padding-top"] = order++;
        map["padding-right"] = order++;
        map["padding-bottom"] = order++;
        map["padding-left"] = order++;

        // Text alignment
        map["text-align"] = order++;
        map["text-indent"] = order++;
        map["vertical-align"] = order++;

        // Font
        map["font-family"] = order++;
        map["font-size"] = order++;
        map["line-height"] = order++;
        map["font-weight"] = order++;
        map["letter-spacing"] = order++;
        map["text-wrap"] = order++;
        map["overflow-wrap"] = order++;
        map["word-break"] = order++;
        map["text-overflow"] = order++;
        map["hyphens"] = order++;
        map["white-space"] = order++;

        // Text color (comes AFTER padding!)
        map["color"] = order++;

        // Text decoration
        map["text-transform"] = order++;
        map["font-style"] = order++;
        map["font-stretch"] = order++;
        map["font-variant-numeric"] = order++;
        map["text-decoration-line"] = order++;
        map["text-decoration-color"] = order++;
        map["text-decoration-style"] = order++;
        map["text-decoration-thickness"] = order++;
        map["text-underline-offset"] = order++;
        map["-webkit-font-smoothing"] = order++;

        // Placeholder
        map["placeholder-color"] = order++;

        // Caret and accent
        map["caret-color"] = order++;
        map["accent-color"] = order++;

        // Color scheme
        map["color-scheme"] = order++;

        // Opacity
        map["opacity"] = order++;

        // Blend modes
        map["background-blend-mode"] = order++;
        map["mix-blend-mode"] = order++;

        // Shadows
        map["box-shadow"] = order++;
        map["--tw-shadow"] = order++;
        map["--tw-shadow-color"] = order++;
        map["--tw-ring-shadow"] = order++;
        map["--tw-ring-color"] = order++;
        map["--tw-inset-shadow"] = order++;
        map["--tw-inset-shadow-color"] = order++;
        map["--tw-inset-ring-shadow"] = order++;
        map["--tw-inset-ring-color"] = order++;
        map["--tw-ring-offset-width"] = order++;
        map["--tw-ring-offset-color"] = order++;

        // Outline
        map["outline"] = order++;
        map["outline-width"] = order++;
        map["outline-offset"] = order++;
        map["outline-color"] = order++;

        // Filters
        map["--tw-blur"] = order++;
        map["--tw-brightness"] = order++;
        map["--tw-contrast"] = order++;
        map["--tw-drop-shadow"] = order++;
        map["--tw-grayscale"] = order++;
        map["--tw-hue-rotate"] = order++;
        map["--tw-invert"] = order++;
        map["--tw-saturate"] = order++;
        map["--tw-sepia"] = order++;
        map["filter"] = order++;

        // Backdrop filters
        map["--tw-backdrop-blur"] = order++;
        map["--tw-backdrop-brightness"] = order++;
        map["--tw-backdrop-contrast"] = order++;
        map["--tw-backdrop-grayscale"] = order++;
        map["--tw-backdrop-hue-rotate"] = order++;
        map["--tw-backdrop-invert"] = order++;
        map["--tw-backdrop-opacity"] = order++;
        map["--tw-backdrop-saturate"] = order++;
        map["--tw-backdrop-sepia"] = order++;
        map["backdrop-filter"] = order++;

        // Transitions
        map["transition-property"] = order++;
        map["transition-behavior"] = order++;
        map["transition-delay"] = order++;
        map["transition-duration"] = order++;
        map["transition-timing-function"] = order++;

        // Will change
        map["will-change"] = order++;
        map["contain"] = order++;

        // Content
        map["content"] = order++;

        // Forced color
        map["forced-color-adjust"] = order;

        return map;
    }
}