using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Transitions;

/// <summary>
/// The transition plugin.
/// </summary>
public class Transition : IUtilityNamespacePlugin
{
    private const string Namespace = "transition";

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals(Namespace))
        {
            yield break;
        }

        var suffix = namespaceSyntax.Suffix ?? "DEFAULT";

        var declarationList = suffix switch
        {
            "none" => new CssDeclarationList { ("transition-property", "none") },
            "all" => new CssDeclarationList
            {
                ("transition-property", "all"),
                ("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)"),
                ("transition-duration", "150ms"),
            },
            "colors" => new CssDeclarationList
            {
                ("transition-property", "color, background-color, border-color, text-decoration-color, fill, stroke"),
                ("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)"),
                ("transition-duration", "150ms"),
            },
            "opacity" => new CssDeclarationList
            {
                ("transition-property", "opacity"),
                ("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)"),
                ("transition-duration", "150ms"),
            },
            "shadow" => new CssDeclarationList
            {
                ("transition-property", "box-shadow"),
                ("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)"),
                ("transition-duration", "150ms"),
            },
            "transform" => new CssDeclarationList
            {
                ("transition-property", "transform"),
                ("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)"),
                ("transition-duration", "150ms"),
            },
            "DEFAULT" => new CssDeclarationList
            {
                ("transition-property", "color, background-color, border-color, text-decoration-color, fill, stroke, opacity, box-shadow, transform, filter, backdrop-filter;"),
                ("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)"),
                ("transition-duration", "150ms"),
            },
            _ => null,
        };

        if (declarationList == null)
        {
            yield break;
        }

        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarationList);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        yield break;
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => new[] { Namespace }.ToImmutableArray();
}