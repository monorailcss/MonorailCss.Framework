using MonorailCss.Css;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The word break plugin.
/// </summary>
public class WordBreak : IUtilityPlugin
{
    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not UtilitySyntax utilitySyntax)
        {
            yield break;
        }

        CssDeclarationList declarations;
        switch (utilitySyntax.Name)
        {
            case "break-normal":
                declarations = new CssDeclarationList()
                {
                    new("overflow-wrap", "normal"), new("break-words", "normal"),
                };
                break;
            case "break-words":
                declarations = new CssDeclarationList() { new("overflow-wrap", "break-word"), };
                break;
            case "break-all":
                declarations = new CssDeclarationList() { new("word-break", "break-all"), };
                break;
            default:
                yield break;
        }

        yield return new CssRuleSet(utilitySyntax.OriginalSyntax, declarations);
    }
}