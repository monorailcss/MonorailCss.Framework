using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Backgrounds;

/// <summary>
/// Background attachment plugin.
/// </summary>
public class BackgroundAttachment : IUtilityNamespacePlugin
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
                    (CssProperties.BackgroundAttachment, value),
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
            { "bg-fixed", "fixed" },
            { "bg-local", "local" },
            { "bg-scroll", "scroll" },
        };

        foreach (var value in values)
        {
            var declarations = new CssDeclarationList
            {
                (CssProperties.BackgroundAttachment, value.Value),
            };
            yield return new CssRuleSet(value.Key, declarations);
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "bg" }];

    private static string? GetValue(string suffix) => suffix switch
    {
        "fixed" => "fixed",
        "local" => "local",
        "scroll" => "scroll",
        _ => null,
    };
}