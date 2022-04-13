using System.Collections.Immutable;

namespace MonorailCss.Plugins.Sizing;

/// <summary>
/// The max-width plugin.
/// </summary>
public class MaxWidth : BaseUtilityNamespacePlugin
{
    private const string Namespace = "max-w";

    private readonly DesignSystem _designSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaxWidth"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public MaxWidth(DesignSystem designSystem)
    {
        _designSystem = designSystem;
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new() { new(Namespace, "max-width"), };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues()
    {
        return new Dictionary<string, string>
            {
                { "none", "none" },
                { "0", "0rem" },
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
                { "full", "100%" },
                { "min", "min-content" },
                { "max", "max-content" },
                { "fit", "fit-content" },
                { "prose", "65ch" },
            }.ToImmutableDictionary()
            .AddRange(_designSystem.Screens.ToImmutableDictionary(i => $"screen-{i.Key}", i => i.Value));
    }
}