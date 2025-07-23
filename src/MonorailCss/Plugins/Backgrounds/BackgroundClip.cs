using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Backgrounds;

/// <summary>
/// Background clip plugin.
/// </summary>
public class BackgroundClip : IUtilityNamespacePlugin
{
    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        // Only process NamespaceSyntax with specific values, not ArbitraryValueSyntax
        if (syntax is NamespaceSyntax namespaceSyntax &&
            namespaceSyntax.NamespaceEquals("bg-clip") &&
            namespaceSyntax.Suffix != null)
        {
            var value = GetValue(namespaceSyntax.Suffix);
            if (value != null)
            {
                var declarations = new CssDeclarationList
                {
                    (CssProperties.BackgroundClip, value),
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
            { "bg-clip-border", "border-box" },
            { "bg-clip-padding", "padding-box" },
            { "bg-clip-content", "content-box" },
            { "bg-clip-text", "text" },
        };

        foreach (var value in values)
        {
            var declarations = new CssDeclarationList
            {
                (CssProperties.BackgroundClip, value.Value),
            };
            yield return new CssRuleSet(value.Key, declarations);
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..new[] { "bg-clip" }];

    private static string? GetValue(string suffix) => suffix switch
    {
        "border" => "border-box",
        "padding" => "padding-box",
        "content" => "content-box",
        "text" => "text",
        _ => null,
    };
}