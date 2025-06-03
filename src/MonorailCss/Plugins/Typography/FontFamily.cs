using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The font-family plugin supporting font-feature-settings and font-variation-settings.
/// </summary>
public class FontFamily : IUtilityPlugin
{
    private readonly DesignSystem _designSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="FontFamily"/> class.
    /// </summary>
    /// <param name="designSystem">The design system to use, or null for default.</param>
    public FontFamily(DesignSystem? designSystem = null)
    {
        _designSystem = designSystem ?? DesignSystem.Default;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not UtilitySyntax utilitySyntax)
        {
            yield break;
        }

        var key = utilitySyntax.Name.Replace("font-", string.Empty);
        if (!_designSystem.FontFamilies.TryGetValue(key, out var fontDef))
        {
            yield break;
        }

        var declarations = new CssDeclarationList();
        declarations.Add(("font-family", fontDef.FontFamily));
        if (!string.IsNullOrWhiteSpace(fontDef.FontFeatureSettings))
        {
            declarations.Add(("font-feature-settings", fontDef.FontFeatureSettings));
        }

        if (!string.IsNullOrWhiteSpace(fontDef.FontVariationSettings))
        {
            declarations.Add(("font-variation-settings", fontDef.FontVariationSettings));
        }

        yield return new CssRuleSet(utilitySyntax.OriginalSyntax, declarations);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var (key, fontDef) in _designSystem.FontFamilies)
        {
            var declarations = new CssDeclarationList();
            declarations.Add(("font-family", fontDef.FontFamily));
            if (!string.IsNullOrWhiteSpace(fontDef.FontFeatureSettings))
            {
                declarations.Add(("font-feature-settings", fontDef.FontFeatureSettings));
            }

            if (!string.IsNullOrWhiteSpace(fontDef.FontVariationSettings))
            {
                declarations.Add(("font-variation-settings", fontDef.FontVariationSettings));
            }

            yield return new CssRuleSet($"font-{key}", declarations);
        }
    }
}