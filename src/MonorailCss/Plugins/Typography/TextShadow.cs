using System.Collections.Generic;
using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text-shadow plugin.
/// </summary>
public class TextShadow : IUtilityPlugin
{
    private readonly ImmutableDictionary<string, CssDeclarationList> _lookups = new Dictionary<string, CssDeclarationList>
    {
        { "text-shadow-sm", [(CssProperties.TextShadow, "0 1px 2px rgb(0 0 0 / 0.05)")] },
        { "text-shadow", [(CssProperties.TextShadow, "0 1px 3px rgb(0 0 0 / 0.1), 0 1px 2px rgb(0 0 0 / 0.1)")] },
        { "text-shadow-md", [(CssProperties.TextShadow, "0 4px 6px rgb(0 0 0 / 0.1), 0 2px 4px rgb(0 0 0 / 0.1)")] },
        { "text-shadow-lg", [(CssProperties.TextShadow, "0 10px 15px rgb(0 0 0 / 0.1), 0 4px 6px rgb(0 0 0 / 0.1)")] },
        { "text-shadow-xl", [(CssProperties.TextShadow, "0 20px 25px rgb(0 0 0 / 0.1), 0 8px 10px rgb(0 0 0 / 0.1)")] },
        { "text-shadow-2xl", [(CssProperties.TextShadow, "0 25px 50px rgb(0 0 0 / 0.25)")] },
        { "text-shadow-none", [(CssProperties.TextShadow, "none")] },
    }.ToImmutableDictionary();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        string? lookupKey = null;

        // Handle both UtilitySyntax and NamespaceSyntax
        if (syntax is UtilitySyntax utilitySyntax)
        {
            lookupKey = utilitySyntax.Name;
        }
        else if (syntax is NamespaceSyntax namespaceSyntax && namespaceSyntax.Namespace == "text")
        {
            // Handle case where "text-shadow-lg" is parsed as namespace "text" with suffix "shadow-lg"
            var suffix = namespaceSyntax.Suffix;
            if (suffix != null && (suffix.StartsWith("shadow") || suffix == "shadow"))
            {
                // Reconstruct the full class name
                lookupKey = $"text-{suffix}";
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