using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The output plugin.
/// </summary>
public class Outline : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, string> _utilities = new Dictionary<string, string>
    {
        { "DEFAULT", "solid" },
        { "dashed", "dashed" },
        { "dotted", "dotted" },
        { "double", "double" },
    }.ToImmutableDictionary();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals("outline"))
        {
            yield break;
        }

        var suffix = namespaceSyntax.Suffix ?? "DEFAULT";
        var declarations = GetDeclarations(suffix);
        if (declarations == default)
        {
            yield break;
        }

        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
    }

    private CssDeclarationList? GetDeclarations(string suffix)
    {
        if (suffix.Equals("none", StringComparison.Ordinal))
        {
            return new CssDeclarationList()
            {
                new(CssProperties.Outline, "2px solid transparent"), new(CssProperties.OutlineOffset, "2px"),
            };
        }

        if (!_utilities.TryGetValue(suffix, out var outline))
        {
            return default;
        }

        var declarations = new CssDeclarationList
        {
            new(CssProperties.OutlineStyle, outline),
        };
        return declarations;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var utility in _utilities)
        {
            var declarations = GetDeclarations(utility.Key);
            if (declarations == default)
            {
                continue;
            }

            var selector = "outline";
            if (utility.Key != "DEFAULT")
            {
                selector += $"-{utility.Key}";
            }

            yield return new CssRuleSet(selector, declarations);
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => new[] { "outline" }.ToImmutableArray();
}