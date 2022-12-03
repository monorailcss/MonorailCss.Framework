﻿namespace MonorailCss.Plugins.Interactivity;

/// <summary>
/// The cursor plugin.
/// </summary>
public class Cursor : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList()
    {
        return new CssNamespaceToPropertyMap("cursor", "cursor");
    }

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return new CssSuffixToValueMap()
        {
            {
                "auto", "auto"
            },
            {
                "default", "default"
            },
            {
                "pointer", "pointer"
            },
            {
                "wait", "wait"
            },
            {
                "text", "text"
            },
            {
                "move", "move"
            },
            {
                "help", "help"
            },
            {
                "not-allowed", "not-allowed"
            },
            {
                "none", "none"
            },
            {
                "context-menu", "context-menu"
            },
            {
                "progress", "progress"
            },
            {
                "cell", "cell"
            },
            {
                "crosshair", "crosshair"
            },
            {
                "vertical-text", "vertical-text"
            },
            {
                "alias", "alias"
            },
            {
                "copy", "copy"
            },
            {
                "no-drop", "no-drop"
            },
            {
                "grab", "grab"
            },
            {
                "grabbing", "grabbing"
            },
            {
                "all-scroll", "all-scroll"
            },
            {
                "col-resize", "col-resize"
            },
            {
                "row-resize", "row-resize"
            },
            {
                "n-resize", "n-resize"
            },
            {
                "e-resize", "e-resize"
            },
            {
                "s-resize", "s-resize"
            },
            {
                "w-resize", "w-resize"
            },
            {
                "ne-resize", "ne-resize"
            },
            {
                "nw-resize", "nw-resize"
            },
            {
                "se-resize", "se-resize"
            },
            {
                "sw-resize", "sw-resize"
            },
            {
                "ew-resize", "ew-resize"
            },
            {
                "ns-resize", "ns-resize"
            },
            {
                "nesw-resize", "nesw-resize"
            },
            {
                "nwse-resize", "nwse-resize"
            },
            {
                "zoom-in", "zoom-in"
            },
            {
                "zoom-out", "zoom-out"
            },
        };
    }
}