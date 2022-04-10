using MonorailCss.Css;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The font-smoothing plugin.
/// </summary>
public class FontSmoothing : IUtilityPlugin
{
    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not UtilitySyntax utilitySyntax)
        {
            yield break;
        }

        CssDeclarationList declaration;
        if (utilitySyntax.Name == "antialiased")
        {
            declaration = new CssDeclarationList()
            {
                new("-webkit-font-smoothing", "antialiased"),
                new("-moz-osx-font-smoothing", "grayscale"),
            };
        }
        else if (utilitySyntax.Name == "subpixel-antialiased")
        {
            declaration = new CssDeclarationList()
            {
                new("-webkit-font-smoothing", "auto"),
                new("-moz-osx-font-smoothing", "auto"),
            };
        }
        else
        {
            yield break;
        }

        yield return new CssRuleSet(utilitySyntax.OriginalSyntax, declaration);
    }
}