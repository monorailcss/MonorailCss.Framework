using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling how elements end within grid columns.
/// </summary>
internal class GridColumnEndUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["col-end"];
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
            new Declaration("grid-column-end", value, important));
    }

    public string[]? GetDocumentedProperties() => ["grid-column-end"];

    public override IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return new[]
        {
            new Documentation.UtilityExample("col-end-1", "End at column line 1"),
            new Documentation.UtilityExample("col-end-7", "End at column line 7"),
            new Documentation.UtilityExample("col-end-13", "End at column line 13"),
            new Documentation.UtilityExample("col-end-auto", "Auto end column"),
            new Documentation.UtilityExample("-col-end-1", "End at column line -1 (from end)"),
            new Documentation.UtilityExample("col-end-[14]", "Use an arbitrary value for column end"),
        };
    }
}