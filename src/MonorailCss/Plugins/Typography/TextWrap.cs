namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text-wrap plugin.
/// </summary>
public class TextWrap : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new("text", "text-wrap");

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues() =>
        new()
        {
            { "wrap", "wrap" },
            { "nowrap", "nowrap" },
            { "balance", "balance" },
            { "pretty", "pretty" },
        };
}