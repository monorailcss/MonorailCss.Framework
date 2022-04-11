namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The border-width plugin.
/// </summary>
public class BorderWidth : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        new()
        {
            { "border", "border-width" },
            { "border-x", ("border-left-width", "border-right-width") },
            { "border-y", ("border-top-width", "border-bottom-width") },
            { "border-r", "border-right-width" },
            { "border-t", "border-top-width" },
            { "border-b", "border-bottom-width" },
            { "border-l", "border-left-width" },
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues() =>
        new()
        {
            { "0", "0px" },
            { "DEFAULT", "1px" },
            { "2", "2px" },
            { "4", "4px" },
            { "8", "8px" },
        };
}