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
        var mapping = namespacePropertyMapList[namespaceSyntax.Namespace];
        var declarationList = CssDeclarationList(value, mapping.Values.Values);

        yield return new CssRuleSet(GetSelector(namespaceSyntax), declarationList, mapping.Importance);
    }

    private static CssDeclarationList CssDeclarationList(string value, string[] propsValues)
    {
        var declarationList = new CssDeclarationList();
        foreach (var property in propsValues)
        {
            declarationList.Add(new CssDeclaration(property, value));
        }

        return declarationList;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var (ns, properties, importance) in _namespacePropertyMapList.Value.ToArray())
        {
            foreach (var (suffix, value) in _suffixToValueMap.Value.ToArray())
            {
                yield return new CssRuleSet($"{ns}-{suffix}", CssDeclarationList(value, properties.Values));
            }
        }
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

/// <summary>
/// Helper class to handle common utility namespace plugins.
/// </summary>
public abstract class BaseColorUtilityNamespacePlugin : IUtilityNamespacePlugin
{
    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => _namespaces.Value;

    private readonly Lazy<ImmutableArray<string>> _namespaces;
    private readonly Lazy<CssNamespaceToPropertyMap> _namespacePropertyMapList;
    private readonly Lazy<CssSuffixToValueMap> _suffixToValueMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseColorUtilityNamespacePlugin"/> class.
    /// </summary>
    protected BaseColorUtilityNamespacePlugin()
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
        var (_, propsValues, importance) = namespacePropertyMapList[namespaceSyntax.Namespace];

        var declarationList = CssDeclarationList(value, propsValues.Values);

        yield return new CssRuleSet(GetSelector(namespaceSyntax), declarationList, importance);
    }

    private static CssDeclarationList CssDeclarationList(string value, string[] propsValues)
    {
        var declarationList = new CssDeclarationList();
        foreach (var property in propsValues)
        {
            declarationList.Add(new CssDeclaration(property, value));
        }

        return declarationList;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var (ns, properties, importance) in _namespacePropertyMapList.Value.ToArray())
        {
            foreach (var (suffix, value) in _suffixToValueMap.Value.ToArray())
            {
                yield return new CssRuleSet($"{ns}-{suffix}", CssDeclarationList(value, properties.Values), importance);
            }
        }
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