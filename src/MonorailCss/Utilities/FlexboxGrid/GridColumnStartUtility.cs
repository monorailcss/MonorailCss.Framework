using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling how elements start within grid columns.
/// </summary>
internal class GridColumnStartUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["col-start"];
    protected override string[] ThemeKeys => NamespaceResolver.GridColumnChain;
    protected override bool SupportsNegative => true;

    protected override string? HandleBareValue(string value)
    {
        return value switch
        {
            "auto" => "auto",
            _ => TryParsePosition(value),
        };
    }

    private static string? TryParsePosition(string value)
    {
        if (int.TryParse(value, out var numValue) && numValue >= 1 && numValue <= 13)
        {
            return numValue.ToString();
        }

        return null;
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

        // Allow numeric values
        if (int.TryParse(value, out _))
        {
            return true;
        }

        return false;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("grid-column-start", value, important));
    }
}