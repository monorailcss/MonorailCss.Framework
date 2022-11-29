namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The outline-width plugin.
/// </summary>
public class OutlineOffset : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new()
    {
        new("outline-offset", "outline-offset"),
    };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues() => new()
    {
        { "0", "0px" },
        { "1", "1px" },
        { "2", "2px" },
        { "4", "4px" },
        { "8", "8px" },
    };
}