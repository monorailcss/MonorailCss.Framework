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
}