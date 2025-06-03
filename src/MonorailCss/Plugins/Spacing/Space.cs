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
        _spacing = ImmutableDictionary.Create<string, string>();
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !Namespaces.Any(i => namespaceSyntax.NamespaceEquals(i)))
        {
            yield break;
        }

        if (namespaceSyntax.Suffix == null)
        {
            yield break;
        }

        if (namespaceSyntax.Suffix == "reverse")
        {
            var variable = CssFramework.GetVariableNameWithPrefix("{ns}-reverse");
            yield return new CssRuleSet(GetSelector(namespaceSyntax), new CssDeclarationList { (variable, "1"), });

            yield break;
        }


        var value = $"calc({CssFramework.GetCssVariableWithPrefix("spacing")} * {namespaceSyntax.Suffix})";

        var names = namespaceSyntax.NamespaceEquals("space-x")
            ? ("space-x-reverse", "margin-inline-start", "margin-inline-end")
            : ("space-y-reverse", "margin-block-start", "margin-block-end");

        var declarations = new CssDeclarationList
        {
            (CssFramework.GetVariableNameWithPrefix(names.Item1), "0"),
            (names.Item2, $"calc({CssFramework.GetCssVariableWithPrefix("spacing")} * {namespaceSyntax.Suffix} * {CssFramework.GetCssVariableWithPrefix(names.Item1)})"),
            (names.Item3, $"calc({CssFramework.GetCssVariableWithPrefix("spacing")} * {namespaceSyntax.Suffix} * (1 - {CssFramework.GetCssVariableWithPrefix(names.Item1)}))"),
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
        return new CssSelector(syntax.OriginalSyntax + " > ", ":not(:last-child)");
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => ["space-x", "space-y"];
}