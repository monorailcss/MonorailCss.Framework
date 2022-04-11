using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// Plugin where each full selector maps to a complete CSS declaration list.
/// </summary>
public abstract class BaseLookupPlugin : IUtilityPlugin
{
    private readonly Lazy<ImmutableDictionary<string, CssDeclarationList>> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseLookupPlugin"/> class.
    /// </summary>
    protected BaseLookupPlugin()
    {
        _rules = new Lazy<ImmutableDictionary<string, CssDeclarationList>>(GetLookups);
    }

    /// <summary>
    /// Gets a full list of look up values.
    /// </summary>
    /// <returns>A list of all look up values.</returns>
    protected abstract ImmutableDictionary<string, CssDeclarationList> GetLookups();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not UtilitySyntax utilitySyntax)
        {
            yield break;
        }

        var rules = _rules.Value;
        if (!rules.TryGetValue(utilitySyntax.Name, out var declarationList))
        {
            yield break;
        }

        yield return new CssRuleSet(utilitySyntax.OriginalSyntax, declarationList);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        var rules = _rules.Value;
        foreach (var rule in rules)
        {
            yield return new CssRuleSet(rule.Key, rule.Value);
        }
    }
}