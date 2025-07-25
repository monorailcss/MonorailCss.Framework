using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Sizing;

/// <summary>
/// The aspect-ratio plugin.
/// </summary>
public class AspectRatio : IUtilityNamespacePlugin
{
    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => ["aspect"];

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        switch (syntax)
        {
            case NamespaceSyntax namespaceSyntax when namespaceSyntax.NamespaceEquals("aspect") && namespaceSyntax.Suffix != null:
                {
                    var value = GetValue(namespaceSyntax.Suffix);
                    if (value != null)
                    {
                        yield return new CssRuleSet(
                            namespaceSyntax.OriginalSyntax,
                            [(CssProperties.AspectRatio, value)]);
                    }

                    break;
                }

            case ArbitraryValueSyntax { Namespace: "aspect" } arbitraryValueSyntax:
                {
                    // Handle arbitrary values like aspect-[3/2]
                    // Replace underscores with spaces for values like aspect-[4_/_3]
                    var value = arbitraryValueSyntax.ArbitraryValue.Replace("_", " ");
                    yield return new CssRuleSet(
                        arbitraryValueSyntax.OriginalSyntax,
                        [(CssProperties.AspectRatio, value)]);
                    break;
                }
        }
    }

    private static string? GetValue(string suffix) => suffix switch
    {
        "auto" => "auto",
        "square" => "1 / 1",
        "video" => "16 / 9",
        _ => null,
    };

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        var values = new Dictionary<string, string>
        {
            { "aspect-auto", "auto" },
            { "aspect-square", "1 / 1" },
            { "aspect-video", "16 / 9" },
        };

        foreach (var value in values)
        {
            yield return new CssRuleSet(
                value.Key,
                [(CssProperties.AspectRatio, value.Value)]);
        }
    }
}