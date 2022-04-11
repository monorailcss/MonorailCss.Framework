﻿using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins;

/// <summary>
/// Helper class to handle common utility namespace plugins.
/// </summary>
public abstract class BaseUtilityNamespacePlugin : IUtilityNamespacePlugin
{
    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => _namespaces.Value;

    private readonly Lazy<ImmutableArray<string>> _namespaces;
    private readonly Lazy<CssNamespaceToPropertyMap> _namespacePropertyMapList;
    private readonly Lazy<CssSuffixToValueMap> _suffixToValueMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseUtilityNamespacePlugin"/> class.
    /// </summary>
    protected BaseUtilityNamespacePlugin()
    {
        _namespaces = new Lazy<ImmutableArray<string>>(() => GetNamespacePropertyMapList().Namespaces.ToImmutableArray());
        _suffixToValueMap = new Lazy<CssSuffixToValueMap>(GetValues);
        _namespacePropertyMapList = new Lazy<CssNamespaceToPropertyMap>(GetNamespacePropertyMapList);
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        var namespacePropertyMapList = _namespacePropertyMapList.Value;
        var cssSuffixToValuesMap = _suffixToValueMap.Value;

        // this utility only works for namespace syntax whose namespaces are defined here..
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespacePropertyMapList.ContainsNamespace(namespaceSyntax.Namespace))
        {
            yield break;
        }

        var suffix = namespaceSyntax.Suffix ?? "DEFAULT";

        // gotta have a suffix that's defined.
        if (!cssSuffixToValuesMap.ContainsSuffix(namespaceSyntax.Suffix ?? "DEFAULT"))
        {
            yield break;
        }

        var value = cssSuffixToValuesMap[suffix];
        var props = namespacePropertyMapList[namespaceSyntax.Namespace];

        var declarationList = new CssDeclarationList();
        foreach (var property in props.Values)
        {
            declarationList.Add(new CssDeclaration(property, value));
        }

        yield return new CssRuleSet(GetSelector(namespaceSyntax), declarationList);
    }

    /// <summary>
    /// Gets the selector given a syntax.
    /// </summary>
    /// <param name="namespaceSyntax">The parsed namespace syntax.</param>
    /// <returns>A string with the current selector name.</returns>
    protected virtual string GetSelector(NamespaceSyntax namespaceSyntax)
    {
        return namespaceSyntax.OriginalSyntax;
    }

    /// <summary>
    /// Gets the property mapping lists.
    /// </summary>
    /// <returns>The mapped namespaces to properties.</returns>
    protected abstract CssNamespaceToPropertyMap GetNamespacePropertyMapList();

    /// <summary>
    /// Gets the values to map to the namespace property map.
    /// </summary>
    /// <returns>The mapped values.</returns>
    protected abstract CssSuffixToValueMap GetValues();
}