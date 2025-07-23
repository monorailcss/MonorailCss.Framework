using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text-stroke plugin for -webkit-text-stroke.
/// </summary>
public class TextStroke : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private readonly ImmutableDictionary<string, string> _widths = new Dictionary<string, string>
    {
        { "thin", "thin" },
        { "medium", "medium" },
        { "thick", "thick" },
        { "0", "0" },
        { "1", "1px" },
        { "2", "2px" },
        { "4", "4px" },
        { "8", "8px" },
    }.ToImmutableDictionary();

    /// <summary>
    /// Initializes a new instance of the <see cref="TextStroke"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public TextStroke(DesignSystem designSystem)
    {
        _flattenedColors = designSystem.GetFlattenColors();
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "text-stroke" }];

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals("text-stroke"))
        {
            yield break;
        }

        var suffix = namespaceSyntax.Suffix;
        if (string.IsNullOrEmpty(suffix))
        {
            yield break;
        }

        // Check if it's a width value
        if (_widths.TryGetValue(suffix, out var widthValue))
        {
            yield return new CssRuleSet(
                namespaceSyntax.OriginalSyntax,
                [(CssProperties.WebkitTextStrokeWidth, widthValue)]);
            yield break;
        }

        // Try to parse as a color
        if (_flattenedColors.TryGetValue(suffix, out var color))
        {
            yield return new CssRuleSet(
                namespaceSyntax.OriginalSyntax,
                [(CssProperties.WebkitTextStrokeColor, color.AsString())]);
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        // Generate width rules
        foreach (var width in _widths)
        {
            yield return new CssRuleSet(
                $"text-stroke-{width.Key}",
                [(CssProperties.WebkitTextStrokeWidth, width.Value)]);
        }

        // Generate color rules
        foreach (var color in _flattenedColors)
        {
            yield return new CssRuleSet(
                $"text-stroke-{color.Key}",
                [(CssProperties.WebkitTextStrokeColor, color.Value.AsString())]);
        }
    }
}