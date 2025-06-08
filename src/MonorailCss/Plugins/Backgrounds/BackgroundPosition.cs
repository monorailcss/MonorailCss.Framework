using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Backgrounds;

/// <summary>
/// Background position plugin.
/// </summary>
public class BackgroundPosition : IUtilityNamespacePlugin
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
                    (CssProperties.BackgroundPosition, value),
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
            { "bg-bottom", "bottom" },
            { "bg-center", "center" },
            { "bg-left", "left" },
            { "bg-left-bottom", "left bottom" },
            { "bg-left-top", "left top" },
            { "bg-right", "right" },
            { "bg-right-bottom", "right bottom" },
            { "bg-right-top", "right top" },
            { "bg-top", "top" },
        };

        foreach (var value in values)
        {
            var declarations = new CssDeclarationList
            {
                (CssProperties.BackgroundPosition, value.Value),
            };
            yield return new CssRuleSet(value.Key, declarations);
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "bg" }];

    private static string? GetValue(string suffix) => suffix switch
    {
        "bottom" => "bottom",
        "center" => "center",
        "left" => "left",
        "left-bottom" => "left bottom",
        "left-top" => "left top",
        "right" => "right",
        "right-bottom" => "right bottom",
        "right-top" => "right top",
        "top" => "top",
        _ => null,
    };
}