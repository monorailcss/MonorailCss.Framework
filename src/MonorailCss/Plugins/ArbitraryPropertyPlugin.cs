using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins;

/// <summary>
/// Special plugin for handling arbitrary properties.
/// </summary>
public class ArbitraryPropertyPlugin : IUtilityPlugin
{
    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not ArbitraryPropertySyntax arbitraryPropertySyntax)
        {
            yield break;
        }

        yield return new CssRuleSet(arbitraryPropertySyntax.OriginalSyntax, [
            (arbitraryPropertySyntax.PropertyName, arbitraryPropertySyntax.ArbitraryValue),
        ]);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        yield break;
    }
}