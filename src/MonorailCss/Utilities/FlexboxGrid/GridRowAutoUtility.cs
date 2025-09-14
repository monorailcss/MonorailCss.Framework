using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utility for grid row auto values.
/// Handles: row-auto
/// CSS: grid-row: auto.
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
}