using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The blur filter plugin.
/// </summary>
public class Blur : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, string> _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="Blur"/> class.
    /// </summary>
    public Blur()
    {
        _values = new Dictionary<string, string>
        {
            { "xs", "4px" },
            { "sm", "8px" },
            { string.Empty, "8px" },
            { "md", "12px" },
            { "lg", "16px" },
            { "xl", "24px" },
            { "2xl", "40px" },
            { "3xl", "64px" },
            { "none", string.Empty },
        }.ToImmutableDictionary();
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => ["blur"];

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        switch (syntax)
        {
            case ArbitraryValueSyntax arbitraryValueSyntax when arbitraryValueSyntax.Namespace.Equals("blur"):
                {
                    var value = ProcessValue(arbitraryValueSyntax.ArbitraryValue);
                    yield return new CssRuleSet(arbitraryValueSyntax.OriginalSyntax, [(CssProperties.Filter, value)]);
                    break;
                }

            case NamespaceSyntax namespaceSyntax when namespaceSyntax.NamespaceEquals("blur"):
                {
                    var suffix = namespaceSyntax.Suffix ?? string.Empty;

                    if (!_values.TryGetValue(suffix, out var rawValue))
                    {
                        yield break;
                    }

                    var value = ProcessValue(rawValue);
                    yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, [(CssProperties.Filter, value)]);
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
            var className = kvp.Key == string.Empty ? "blur" : $"blur-{kvp.Key}";
            var value = ProcessValue(kvp.Value);
            yield return new CssRuleSet(className, [(CssProperties.Filter, value)]);
        }
    }

    private static string ProcessValue(string value)
    {
        if (value == string.Empty || value == "none")
        {
            return string.Empty;
        }

        return $"blur({value})";
    }
}