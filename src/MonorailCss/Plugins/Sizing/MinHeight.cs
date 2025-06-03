using System.Collections.Immutable;

namespace MonorailCss.Plugins.Sizing;

/// <summary>
/// The max-width plugin.
/// </summary>
public class MinHeight : BaseUtilityNamespacePlugin
{
    private const string Namespace = "min-h";

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
    [
        new(Namespace, "min-height"),
    ];

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return new Dictionary<string, string>
        {
            { "auto", "auto" },
            { "px", "1px" },
            { "full", "100%" },
            { "screen", "100vh" },
            { "dvh", "100dvh" },
            { "dvw", "100dvw" },
            { "lvh", "100lvh" },
            { "lvw", "100lvw" },
            { "svh", "100svh" },
            { "svw", "100svw" },
            { "min", "min-content" },
            { "max", "max-content" },
            { "fit", "fit-content" },
            { "lh", "1lh" },
            { "0", "0px" },
        }.ToImmutableDictionary();
    }
}