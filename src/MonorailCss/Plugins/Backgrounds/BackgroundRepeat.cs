using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Backgrounds;

/// <summary>
/// Background repeat plugin.
/// </summary>
public class BackgroundRepeat : IUtilityNamespacePlugin
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
                    (CssProperties.BackgroundRepeat, value),
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
            { "bg-repeat", "repeat" },
            { "bg-no-repeat", "no-repeat" },
            { "bg-repeat-x", "repeat-x" },
            { "bg-repeat-y", "repeat-y" },
            { "bg-repeat-round", "round" },
            { "bg-repeat-space", "space" },
        };

        foreach (var value in values)
        {
            var declarations = new CssDeclarationList
            {
                (CssProperties.BackgroundRepeat, value.Value),
            };
            yield return new CssRuleSet(value.Key, declarations);
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "bg" }];

    private static string? GetValue(string suffix) => suffix switch
    {
        "repeat" => "repeat",
        "no-repeat" => "no-repeat",
        "repeat-x" => "repeat-x",
        "repeat-y" => "repeat-y",
        "repeat-round" => "round",
        "repeat-space" => "space",
        _ => null,
    };
}