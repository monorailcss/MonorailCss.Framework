using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Layout;

/// <summary>
/// The content plugin for controlling the content of before and after pseudo-elements.
/// </summary>
public class Content : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, string> _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="Content"/> class.
    /// </summary>
    public Content()
    {
        _values = new Dictionary<string, string>
        {
            { "none", "none" },
        }.ToImmutableDictionary();
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => ["content"];

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        switch (syntax)
        {
            case ArbitraryValueSyntax arbitraryValueSyntax when arbitraryValueSyntax.Namespace.Equals("content"):
                {
                    var value = ProcessArbitraryValue(arbitraryValueSyntax.ArbitraryValue);
                    yield return new CssRuleSet(arbitraryValueSyntax.OriginalSyntax, [(CssProperties.Content, value)]);
                    break;
                }

            case NamespaceSyntax namespaceSyntax when namespaceSyntax.NamespaceEquals("content"):
                {
                    var suffix = namespaceSyntax.Suffix ?? string.Empty;

                    // Handle custom properties: content-(--custom-prop)
                    if (suffix.StartsWith("(--") && suffix.EndsWith(")"))
                    {
                        var customProperty = suffix[1..^1]; // Remove parentheses
                        var value = $"var({customProperty})";
                        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, [(CssProperties.Content, value)]);
                        break;
                    }

                    if (!_values.TryGetValue(suffix, out var rawValue))
                    {
                        yield break;
                    }

                    yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, [(CssProperties.Content, rawValue)]);
                    break;
                }

            default:
                yield break;
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var kvp in _values)
        {
            var className = $"content-{kvp.Key}";
            yield return new CssRuleSet(className, [(CssProperties.Content, kvp.Value)]);
        }
    }

    private static string ProcessArbitraryValue(string arbitraryValue)
    {
        // Handle underscore to space conversion for content values
        return arbitraryValue.Replace("_", " ");
    }
}