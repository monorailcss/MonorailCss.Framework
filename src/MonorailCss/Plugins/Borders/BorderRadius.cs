using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The border-radius plugin.
/// </summary>
public class BorderRadius : IUtilityNamespacePlugin
{
    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax)
        {
            yield break;
        }

        if (!_namespaceToPropertyMap.ContainsNamespace(namespaceSyntax.Namespace))
        {
            yield break;
        }

        var properties = _namespaceToPropertyMap[namespaceSyntax.Namespace];

        var suffix = namespaceSyntax.Suffix ?? "DEFAULT";
        if (!_sizes.TryGetValue(suffix, out var size))
        {
            yield break;
        }

        var cssDeclaration = new CssDeclarationList();
        foreach (var property in properties.Values)
        {
            cssDeclaration.Add(new CssDeclaration(property, size));
        }

        yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, cssDeclaration);
    }

    private readonly ImmutableDictionary<string, string> _sizes = new Dictionary<string, string>()
    {
        { "none", "0px" },
        { "sm", "0.125rem" },
        { "DEFAULT", "0.25rem" },
        { "md", "0.375rem" },
        { "lg", "0.5rem" },
        { "xl", "0.75rem" },
        { "2xl", "1rem" },
        { "3xl", "1.5rem" },
        { "full", "9999px" },
    }.ToImmutableDictionary();

    private readonly CssNamespaceToPropertyMap _namespaceToPropertyMap = new()
    {
        { "rounded", "border-radius" },
        { "rounded-t", ("border-top-left-radius", "border-top-right-radius") },
        { "rounded-r", ("border-top-right-radius", "border-bottom-right-radius") },
        { "rounded-b", ("border-bottom-right-radius", "border-bottom-left-radius") },
        { "rounded-l", ("border-top-left-radius", "border-bottom-left-radius") },
        { "rounded-tl", "border-top-left-radius" },
        { "rounded-tr", "border-top-right-radius" },
        { "rounded-br", "border-bottom-right-radius" },
        { "rounded-bl", "border-bottom-left-radius" },
    };

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => _namespaceToPropertyMap.Namespaces.ToImmutableArray();
}

/// <summary>
/// The border-width plugin.
/// </summary>
public class BorderWidth : BaseUtilityNamespacePlugin
{
    /// <inheritdoc />
    protected override CssNamespaceToPropertyMap GetNamespacePropertyMapList() =>
        new()
        {
            { "border", "border-width" },
            { "border-x", ("border-left-width", "border-right-width") },
            { "border-y", ("border-top-width", "border-bottom-width") },
            { "border-r", "border-right-width" },
            { "border-t", "border-top-width" },
            { "border-b", "border-bottom-width" },
            { "border-l", "border-left-width" },
        };

    /// <inheritdoc />
    protected override CssSuffixToValueMap GetValues() =>
        new()
        {
            { "0", "0px" },
            { "DEFAULT", "1px" },
            { "2", "2px" },
            { "4", "4px" },
            { "8", "8px" },
        };
}