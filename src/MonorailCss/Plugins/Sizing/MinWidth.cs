using System.Collections.Immutable;

namespace MonorailCss.Plugins.Sizing;

/// <summary>
/// The max-width plugin.
/// </summary>
public class MinWidth : BaseUtilityNamespacePlugin
{
    private const string Namespace = "min-w";

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new(Namespace, "min-width");

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return new Dictionary<string, string>
        {
            { "3xs", "16rem" },
            { "2xs", "18rem" },
            { "xs", "20rem" },
            { "sm", "24rem" },
            { "md", "28rem" },
            { "lg", "32rem" },
            { "xl", "36rem" },
            { "2xl", "42rem" },
            { "3xl", "48rem" },
            { "4xl", "56rem" },
            { "5xl", "64rem" },
            { "6xl", "72rem" },
            { "7xl", "80rem" },
            { "auto", "auto" },
            { "px", "1px" },
            { "0", "0px" },
            { "full", "100%" },
            { "screen", "100vw" },
            { "dvw", "100dvw" },
            { "dvh", "100dvh" },
            { "lvw", "100lvw" },
            { "lvh", "100lvh" },
            { "svw", "100svw" },
            { "svh", "100svh" },
            { "min", "min-content" },
            { "max", "max-content" },
            { "fit", "fit-content" }
        }.ToImmutableDictionary();
    }
}