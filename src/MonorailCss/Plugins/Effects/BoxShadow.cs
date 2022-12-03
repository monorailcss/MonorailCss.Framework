using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Effects;

/// <summary>
/// The box-shadow plugin.
/// </summary>
public class BoxShadow : IUtilityNamespacePlugin, IRegisterDefaults
{
    private readonly ImmutableDictionary<string, (string Shadow, string Color)> _utilities = new Dictionary<string, (string, string)>
    {
        { "sm", ("0 1px 2px 0 COLOR_PLACE_HOLDER", "rgb(0 0 0 / 0.05)") },
        { "DEFAULT", ("0 1px 3px 0 COLOR_PLACE_HOLDER, 0 1px 2px -1px COLOR_PLACE_HOLDER", "rgb(0 0 0 / 0.1)") },
        { "md", ("0 4px 6px -1px COLOR_PLACE_HOLDER, 0 2px 4px -2px COLOR_PLACE_HOLDER", "rgb(0 0 0 / 0.05)") },
        { "lg", ("0 10px 15px -3px COLOR_PLACE_HOLDER, 0 4px 6px -4px COLOR_PLACE_HOLDER", "rgb(0 0 0 / 0.05)") },
        { "xl", ("0 20px 25px -5px COLOR_PLACE_HOLDER, 0 8px 10px -6px COLOR_PLACE_HOLDER", "rgb(0 0 0 / 0.05)") },
        { "2xl", ("0 25px 50px -12px COLOR_PLACE_HOLDER", "rgb(0 0 0 / 0.25)") },
        { "inner", ("inset 0 2px 4px 0 COLOR_PLACE_HOLDER", "rgb(0 0 0 / 0.25)") },
        { "none", ("(0 0 #0000", string.Empty) },
    }.ToImmutableDictionary();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        if (syntax is not NamespaceSyntax namespaceSyntax || !namespaceSyntax.NamespaceEquals("shadow"))
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
        if (!_utilities.TryGetValue(suffix, out var shadow))
        {
            return default;
        }

        var color = shadow.Shadow.Replace("COLOR_PLACE_HOLDER", shadow.Color);
        var coloredShadow =
            shadow.Shadow.Replace("COLOR_PLACE_HOLDER", CssFramework.GetVariableNameWithPrefix("shadow-color"));
        var shadowValue = $"{CssFramework.GetCssVariableWithPrefix("ring-offset-shadow")}, 0 0 #0000, {CssFramework.GetCssVariableWithPrefix("ring-shadow")}, 0 0 #0000, {CssFramework.GetCssVariableWithPrefix("shadow")}";
        var declarations = new CssDeclarationList
        {
            new(CssFramework.GetVariableNameWithPrefix("shadow"), color),
            new(CssFramework.GetVariableNameWithPrefix("shadow-colored"), coloredShadow),
            new(
                CssProperties.BoxShadow,
                shadowValue),
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

            var selector = "shadow";
            if (utility.Key != "DEFAULT")
            {
                selector += $"-{utility.Key}";
            }

            yield return new CssRuleSet(selector, declarations);
        }
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => new[] { "shadow" }.ToImmutableArray();

    /// <inheritdoc />
    public CssDeclarationList GetDefaults()
    {
        return new CssDeclarationList
        {
            new(CssFramework.GetVariableNameWithPrefix("ring-offset-shadow"), "0 0 #0000"),
            new(CssFramework.GetVariableNameWithPrefix("ring-shadow"), "0 0 #0000"),
            new(CssFramework.GetVariableNameWithPrefix("shadow"), "0 0 #0000"),
            new(CssFramework.GetVariableNameWithPrefix("shadow-colored"), "0 0 #0000"),
        };
    }
}