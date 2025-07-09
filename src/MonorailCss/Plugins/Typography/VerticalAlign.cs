namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The vertical-align plugin.
/// </summary>
public class VerticalAlign : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new("align", "vertical-align");

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues() =>
        new()
        {
            { "baseline", "baseline" },
            { "top", "top" },
            { "middle", "middle" },
            { "bottom", "bottom" },
            { "text-top", "text-top" },
            { "text-bottom", "text-bottom" },
            { "sub", "sub" },
            { "super", "super" },
        };
}