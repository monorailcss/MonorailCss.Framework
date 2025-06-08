using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Backgrounds;

/// <summary>
/// Background size plugin.
/// </summary>
public class BackgroundSize : IUtilityNamespacePlugin
{
    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        // Only process NamespaceSyntax with specific values, not ArbitraryValueSyntax
        if (syntax is NamespaceSyntax namespaceSyntax &&
            namespaceSyntax.NamespaceEquals("bg") &&
            namespaceSyntax.Suffix != null)
        {
            var value = GetValue(namespaceSyntax.Suffix);
            if (value != null)
            {
                var declarations = new CssDeclarationList
                {
                    (CssProperties.BackgroundSize, value),
                };
                yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        var values = new Dictionary<string, string>
        {
            { "bg-auto", "auto" },
            { "bg-cover", "cover" },
            { "bg-contain", "contain" },
        };

        foreach (var value in values)
        {
            var declarations = new CssDeclarationList
            {
                (CssProperties.BackgroundSize, value.Value),
            };
            yield return new CssRuleSet(value.Key, declarations);
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "bg" }];

    private static string? GetValue(string suffix) => suffix switch
    {
        "auto" => "auto",
        "cover" => "cover",
        "contain" => "contain",
        _ => null,
    };
}