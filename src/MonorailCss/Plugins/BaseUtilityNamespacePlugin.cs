using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins;

/// <summary>
/// Helper class to handle common utility namespace plugins.
/// </summary>
public abstract class BaseUtilityNamespacePlugin : IUtilityNamespacePlugin
{
    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => NamespacePropertyMapList.Namespaces.ToImmutableArray();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        // this utility only works for namespace syntax whose namespaces are defined here..
        if (syntax is not NamespaceSyntax namespaceSyntax || !NamespacePropertyMapList.ContainsNamespace(namespaceSyntax.Namespace))
        {
            yield break;
        }

        var suffix = namespaceSyntax.Suffix ?? "DEFAULT";

        // gotta have a suffix that's defined.
        if (!Values.ContainsSuffix(namespaceSyntax.Suffix ?? "DEFAULT"))
        {
            yield break;
        }

        var value = Values[suffix];
        var props = NamespacePropertyMapList[namespaceSyntax.Namespace];

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
    protected abstract CssNamespaceToPropertyMap NamespacePropertyMapList { get; }

    /// <summary>
    /// Gets the values to map to the namespace property map.
    /// </summary>
    protected abstract CssSuffixToValueMap Values { get; }
}