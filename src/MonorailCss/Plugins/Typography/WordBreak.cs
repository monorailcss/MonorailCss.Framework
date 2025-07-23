using System.Collections.Generic;
using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The word break plugin.
/// </summary>
public class WordBreak : IUtilityPlugin
{
    private readonly ImmutableDictionary<string, CssDeclarationList> _lookups = new Dictionary<string, CssDeclarationList>
    {
        {
            "break-normal", [("overflow-wrap", "normal"), ("break-words", "normal")]
        },
        {
            "break-words", [("overflow-wrap", "break-word")]
        },
        {
            "break-all", [("word-break", "break-all")]
        },
    }.ToImmutableDictionary();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        string? lookupKey = null;

        if (syntax is UtilitySyntax utilitySyntax)
        {
            lookupKey = utilitySyntax.Name;
        }
        else if (syntax is NamespaceSyntax namespaceSyntax && namespaceSyntax.Namespace == "break")
        {
            // Handle case where "break-normal" might be parsed as namespace "break" with suffix "normal"
            var suffix = namespaceSyntax.Suffix;
            if (suffix != null)
            {
                lookupKey = $"{namespaceSyntax.Namespace}-{suffix}";
            }
        }

        if (lookupKey != null && _lookups.TryGetValue(lookupKey, out var declarationList))
        {
            yield return new CssRuleSet(syntax.OriginalSyntax, declarationList);
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var rule in _lookups)
        {
            yield return new CssRuleSet(rule.Key, rule.Value);
        }
    }
}