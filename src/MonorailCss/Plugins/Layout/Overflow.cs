using System.Collections.Immutable;

namespace MonorailCss.Plugins.Layout;

/// <summary>
/// The overflow plugin.
/// </summary>
public class Overflow : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap NamespacePropertyMapList => new CssNamespaceToPropertyMap()
    {
        { "overflow", "overflow" },
        { "overflow-x", "overflow-x " },
        { "overflow-y", "overflow-y " },
    };

    /// <inheritdoc />
    protected override CssSuffixToValueMap Values => new CssSuffixToValueMap()
    {
        { "auto", "auto" },
        { "hidden", "hidden" },
        { "clip", "clip" },
        { "visible", "visible" },
        { "scroll", "scroll" },
    };
}