using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Plugins;

/// <summary>
/// The display plugin.
/// </summary>
public class Display : IUtilityPlugin
{
    private ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "block", "block" },
            { "inline-block", "inline-block" },
            { "inline", "inline" },
            { "flex", "flex" },
            { "inline-flex", "inline-flex" },
            { "table", "table" },
            { "inline-table", "inline-table" },
            { "table-caption", "table-caption" },
            { "table-cell", "table-cell" },
            { "table-column", "table-column" },
            { "table-column-group", "table-column-group" },
            { "table-footer-group", "table-footer-group" },
            { "table-header-group", "table-header-group" },
            { "table-row-group", "table-row-group" },
            { "table-row", "table-row" },
            { "flow-root", "flow-root" },
            { "grid", "grid" },
            { "inline-grid", "inline-grid" },
            { "contents", "contents" },
            { "list-item", "list-item" },
            { "hidden", "none" },
        }.ToImmutableDictionary();

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        var utilities = GetUtilities();
        if (syntax is UtilitySyntax utilitySyntax)
        {
            if (!utilities.TryGetValue(utilitySyntax.Name, out var value))
            {
                yield break;
            }

            yield return new CssRuleSet(
                utilitySyntax.OriginalSyntax,
                new CssDeclarationList() { new("display", value), });
        }
        else if (syntax is NamespaceSyntax namespaceSyntax)
        {
            if (!utilities.TryGetValue(namespaceSyntax.Namespace, out var value))
            {
                yield break;
            }

            yield return new CssRuleSet(
                namespaceSyntax.OriginalSyntax,
                new CssDeclarationList() { new("display", value), });
        }
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        return GetUtilities()
            .Select(utility => new CssRuleSet(
                utility.Key,
                new CssDeclarationList() { new("display", utility.Value) }));
    }
}