﻿using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Spacing;

/// <summary>
/// The space plugin.
/// </summary>
public class Space : IUtilityNamespacePlugin
{
    private readonly CssFramework _framework;
    private readonly ImmutableDictionary<string, string> _spacing;

    /// <summary>
    /// Initializes a new instance of the <see cref="Space"/> class.
    /// </summary>
    /// <param name="framework">The framework.</param>
    /// <param name="designSystem">The design system.</param>
    public Space(CssFramework framework, DesignSystem designSystem)
    {
        _framework = framework;
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
            var variable = _framework.GetVariableNameWithPrefix("{ns}-reverse");
            yield return new CssRuleSet(GetSelector(namespaceSyntax), new CssDeclarationList() { new(variable, "1"), });

            yield break;
        }

        if (!_spacing.TryGetValue(namespaceSyntax.Suffix, out var value))
        {
            yield break;
        }

        var names = namespaceSyntax.NamespaceEquals("space-x")
            ? ("space-x-reverse", "margin-left", "margin-right")
            : ("space-y-reverse", "margin-top", "margin-bottom");

        var declarations = new CssDeclarationList()
        {
            new(_framework.GetVariableNameWithPrefix(names.Item1), "0"),
            new(names.Item2, $"calc({value} * calc(1 - {_framework.GetCssVariableWithPrefix(names.Item1)}))"),
            new(names.Item3, $"calc({value} * {_framework.GetCssVariableWithPrefix(names.Item1)})"),
        };

        yield return new CssRuleSet(GetSelector(syntax), declarations);
    }

    private static CssSelector GetSelector(IParsedClassNameSyntax syntax)
    {
        return new CssSelector(syntax.OriginalSyntax + ">", "not([hidden])~:not([hidden])");
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => new[] { "space-x", "space-y" }.ToImmutableArray();
}