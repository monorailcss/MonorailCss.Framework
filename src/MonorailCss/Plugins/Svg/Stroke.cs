using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Svg;

/// <summary>
/// The fill plugin.
/// </summary>
public class Stroke : BaseUtilityNamespacePlugin
{
    private const string Namespace = "stroke";
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;

    /// <summary>
    /// Initializes a new instance of the <see cref="Stroke"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Stroke(DesignSystem designSystem)
    {
        _flattenedColors = designSystem.GetFlattenColors();
    }

    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() => new(Namespace, "stroke");

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