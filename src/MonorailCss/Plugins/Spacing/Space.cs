using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Spacing;

/// <summary>
/// The space plugin.
/// </summary>
public class Space : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, string> _spacing;

    /// <summary>
    /// Initializes a new instance of the <see cref="Space"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public Space(DesignSystem designSystem)
    {
        _spacing = designSystem.Spacing;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !Namespaces.Any(i => namespaceSyntax.NamespaceEquals(i)))
        {
            yield break;
        }

        if (namespaceSyntax.Suffix == default)
        {
            yield break;
        }

        if (namespaceSyntax.Suffix == "reverse")
        {
            var variable = CssFramework.GetVariableNameWithPrefix("{ns}-reverse");
            yield return new CssRuleSet(GetSelector(namespaceSyntax), new CssDeclarationList { (variable, "1"), });

            yield break;
        }

        if (!_spacing.TryGetValue(namespaceSyntax.Suffix, out var value))
        {
            yield break;
        }

        var names = namespaceSyntax.NamespaceEquals("space-x")
            ? ("space-x-reverse", "margin-left", "margin-right")
            : ("space-y-reverse", "margin-top", "margin-bottom");

        var declarations = new CssDeclarationList
        {
            (CssFramework.GetVariableNameWithPrefix(names.Item1), "0"),
            (names.Item2, $"calc({value} * calc(1 - {CssFramework.GetCssVariableWithPrefix(names.Item1)}))"),
            (names.Item3, $"calc({value} * {CssFramework.GetCssVariableWithPrefix(names.Item1)})"),
        };

        yield return new CssRuleSet(GetSelector(syntax), declarations);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        yield break;
    }

    private static CssSelector GetSelector(IParsedClassNameSyntax syntax)
    {
        return new CssSelector(syntax.OriginalSyntax + " > ", ":not([hidden])~:not([hidden])");
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => new[] { "space-x", "space-y" }.ToImmutableArray();
}