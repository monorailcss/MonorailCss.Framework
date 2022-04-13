namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text-align plugin.
/// </summary>
public class TextAlign : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new("text", "text-align");

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues() =>
        new()
        {
            { "left", "left" }, { "center", "center" }, { "right", "right" }, { "justify", "justify" },
        };
}