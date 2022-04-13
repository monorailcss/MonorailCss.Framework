namespace MonorailCss.Plugins.Svg;

/// <summary>
/// The stroke-width plugin.
/// </summary>
public class StrokeWidth : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList()
    {
        return new CssNamespaceToPropertyMap("stroke", "stroke-width");
    }

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return new CssSuffixToValueMap { { "0", "0" }, { "1", "1" }, { "2", "2" }, };
    }
}