using System.Collections.Immutable;
using MonorailCss.Css;

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
            "none" => new CssDeclarationList { new("transition-property", "none") },
            "all" => new CssDeclarationList
            {
                new("transition-property", "all"),
                new("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)"),
                new("transition-duration", "150ms"),
            },
            "colors" => new CssDeclarationList
            {
                new(
                    "transition-property",
                    "color, background-color, border-color, text-decoration-color, fill, stroke"),
                new("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)"),
                new("transition-duration", "150ms"),
            },
            "opacity" => new CssDeclarationList
            {
                new("transition-property", "opacity"),
                new("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)"),
                new("transition-duration", "150ms"),
            },
            "shadow" => new CssDeclarationList
            {
                new("transition-property", "box-shadow"),
                new("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)"),
                new("transition-duration", "150ms"),
            },
            "transform" => new CssDeclarationList
            {
                new("transition-property", "transform"),
                new("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)"),
                new("transition-duration", "150ms"),
            },
            "DEFAULT" => new CssDeclarationList
            {
                new(
                    "transition-property",
                    "color, background-color, border-color, text-decoration-color, fill, stroke, opacity, box-shadow, transform, filter, backdrop-filter;"),
                new("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)"),
                new("transition-duration", "150ms"),
            },
            _ => default,
        };

        if (declarationList == default)
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