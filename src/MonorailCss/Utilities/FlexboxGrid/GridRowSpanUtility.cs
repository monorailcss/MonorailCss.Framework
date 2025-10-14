using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling how elements span across grid rows.
/// </summary>
internal class GridRowSpanUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["row-span"];
    protected override string[] ThemeKeys => NamespaceResolver.GridRowChain;

    protected override string? HandleBareValue(string value)
    {
        return value switch
        {
            "full" => "1 / -1",
            "auto" => "auto",
            _ => TryParseNumericSpan(value),
        };
    }

    private static string? TryParseNumericSpan(string value)
    {
        if (int.TryParse(value, out var numValue) && numValue >= 1 && numValue <= 12)
        {
            return $"span {numValue} / span {numValue}";
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

    public string[]? GetDocumentedProperties() => ["grid-row"];

    public override IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return new[]
        {
            new Documentation.UtilityExample("row-span-1", "Span 1 row"),
            new Documentation.UtilityExample("row-span-3", "Span 3 rows"),
            new Documentation.UtilityExample("row-span-6", "Span 6 rows"),
            new Documentation.UtilityExample("row-span-full", "Span all rows"),
            new Documentation.UtilityExample("row-span-auto", "Auto span rows"),
            new Documentation.UtilityExample("row-span-[2]", "Use an arbitrary value for row span"),
        };
    }
}