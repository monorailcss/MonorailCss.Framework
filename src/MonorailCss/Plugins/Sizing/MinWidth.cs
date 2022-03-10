using System.Collections.Immutable;

namespace MonorailCss.Plugins.Sizing;

/// <summary>
/// The max-width plugin.
/// </summary>
public class MinWidth : BaseUtilityNamespacePlugin
{
    private const string Namespace = "min-w";

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap NamespacePropertyMapList => new() { { Namespace, "min-width" }, };

    /// <inheritdoc />
    protected override CssSuffixToValueMap Values { get; } = new Dictionary<string, string>()
    {
        { "0", "0px" },
        { "full", "100%" },
        { "min", "min-content" },
        { "max", "max-content" },
        { "fit", "fit-content" },
    }.ToImmutableDictionary();
}