using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for setting grid items to automatically span rows.
/// </summary>
internal class GridRowAutoUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["row"];
    protected override string[] ThemeKeys => NamespaceResolver.GridRowChain;
    protected override string? DefaultValue => null; // No default - only handles specific values

    protected override string? HandleBareValue(string value)
    {
        return value switch
        {
            "auto" => "auto",
            _ => null, // Only handle "auto" for row-auto
        };
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "auto", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow grid line patterns like "1", "1 / 3", "span 2", "span 2 / span 2"
        if (value.Contains('/') || value.StartsWith("span ") || int.TryParse(value, out _))
        {
            return true;
        }

        return false;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("grid-row", value, important));
    }

    public string[] GetDocumentedProperties() => ["grid-row"];

    public override IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return
        [
            new Documentation.UtilityExample("row-auto", "Set grid row to auto"),
            new Documentation.UtilityExample("row-[1_/_3]", "Position element from row line 1 to 3"),
            new Documentation.UtilityExample("row-[value]", "Set grid row with arbitrary value", "grid-row: [value]"),
        ];
    }
}