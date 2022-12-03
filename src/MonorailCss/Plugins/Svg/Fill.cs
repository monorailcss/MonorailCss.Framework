using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Svg;

/// <summary>
/// The fill plugin.
/// </summary>
public class Fill : BaseUtilityNamespacePlugin
{
    private const string Namespace = "fill";
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;

    /// <summary>
    /// Initializes a new instance of the <see cref="Fill"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Fill(DesignSystem designSystem)
    {
        _flattenedColors = designSystem.GetFlattenColors();
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new(Namespace, "fill");

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues() =>
        _flattenedColors.ToImmutableDictionary(k => k.Key, v => v.Value.AsRgb()).AddRange(
            new Dictionary<string, string>
            {
                { "inherit", "inherit" },
                { "current", "currentColor" },
                { "transparent", "transparent" },
            });
}