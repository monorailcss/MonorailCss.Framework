using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling how elements end within grid rows.
/// </summary>
internal class GridRowEndUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["row-end"];
    protected override string[] ThemeKeys => NamespaceResolver.GridRowChain;
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
            new Declaration("grid-row-end", value, important));
    }

    public string[] GetDocumentedProperties() => ["grid-row-end"];

    public override IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return
        [
            new Documentation.UtilityExample("row-end-1", "End at row line 1"),
            new Documentation.UtilityExample("row-end-4", "End at row line 4"),
            new Documentation.UtilityExample("row-end-7", "End at row line 7"),
            new Documentation.UtilityExample("row-end-auto", "Auto end row"),
            new Documentation.UtilityExample("-row-end-1", "End at row line -1 (from end)"),
            new Documentation.UtilityExample("row-end-[8]", "Use an arbitrary value for row end"),
        ];
    }
}