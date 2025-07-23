using System.Collections.Generic;
using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The font-smoothing plugin.
/// </summary>
public class FontSmoothing : IUtilityPlugin
{
    private readonly ImmutableDictionary<string, CssDeclarationList> _lookups = new Dictionary<string, CssDeclarationList>
    {
        {
            "antialiased", [
                ("-webkit-font-smoothing", "antialiased"), ("-moz-osx-font-smoothing", "grayscale"),
            ]
        },
        {
            "subpixel-antialiased", [
                ("-webkit-font-smoothing", "auto"), ("-moz-osx-font-smoothing", "auto"),
            ]
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
        else if (syntax is NamespaceSyntax namespaceSyntax)
        {
            // Handle potential namespace parsing for utilities like "antialiased"
            // In case they get parsed as namespace syntax
            var fullName = namespaceSyntax.Namespace;
            if (namespaceSyntax.Suffix != null)
            {
                fullName = $"{namespaceSyntax.Namespace}-{namespaceSyntax.Suffix}";
            }

            lookupKey = fullName;
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