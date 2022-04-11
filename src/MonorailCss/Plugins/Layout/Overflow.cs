﻿using System.Collections.Immutable;

namespace MonorailCss.Plugins.Layout;

/// <summary>
/// The overflow plugin.
/// </summary>
public class Overflow : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        new()
        {
            { "overflow", "overflow" },
            { "overflow-x", "overflow-x " },
            { "overflow-y", "overflow-y " },
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues() =>
        new()
        {
            { "auto", "auto" },
            { "hidden", "hidden" },
            { "clip", "clip" },
            { "visible", "visible" },
            { "scroll", "scroll" },
        };
}