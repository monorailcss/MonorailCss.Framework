using System.Collections.Immutable;

namespace MonorailCss.Plugins.Sizing;

/// <summary>
/// The max-width plugin.
/// </summary>
public class MinHeight : BaseUtilityNamespacePlugin
{
    private const string Namespace = "min-h";

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap NamespacePropertyMapList => new() { { Namespace, "min-height" }, };

    /// <inheritdoc />
    protected override CssSuffixToValueMap Values { get; } = new Dictionary<string, string>()
    {
        { "0", "0px" },
        { "full", "100%" },
        { "screen", "100vh" },
        { "min", "min-content" },
        { "max", "max-content" },
        { "fit", "fit-content" },
    }.ToImmutableDictionary();
}