namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The outline-width plugin.
/// </summary>
public class OutlineWidth : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
    [
        new("outline-width", "outline-width"),
    ];

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