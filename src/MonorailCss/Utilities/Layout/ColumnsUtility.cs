using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling the number of columns within an element.
/// </summary>
internal class ColumnsUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["columns"];

    protected override string[] ThemeKeys => ["--columns", "--container"];

    /// <summary>
    /// Handles bare named values and converts them to appropriate columns values.
    /// Examples: "auto" -> "auto", "1" -> "1", "sm" -> resolved from theme.
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Handle static values first
        if (value == "auto")
        {
            return "auto";
        }

        // Handle numeric values (columns-1 through columns-12)
        if (int.TryParse(value, out var numValue) && numValue >= 1 && numValue <= 12)
        {
            return numValue.ToString();
        }

        // For container-based values, return null to let theme resolution handle it
        var containerSizes = new[]
        {
            "3xs", "2xs", "xs", "sm", "md", "lg", "xl",
            "2xl", "3xl", "4xl", "5xl", "6xl", "7xl",
        };

        if (containerSizes.Contains(value))
        {
            return null; // Let theme resolution handle it
        }

        return null;
    }

    /// <summary>
    /// Validates arbitrary values for columns.
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "auto", "initial", "inherit", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow positive numbers for column count
        if (int.TryParse(value.Trim(), out var intValue) && intValue > 0)
        {
            return true;
        }

        // Allow length values for column width (rem, em, px, etc.)
        var units = new[] { "rem", "em", "px", "pt", "%", "ch", "vw", "vh" };
        foreach (var unit in units)
        {
            if (value.EndsWith(unit))
            {
                var numericPart = value[..^unit.Length];
                if (double.TryParse(numericPart, out var doubleValue) && doubleValue > 0)
                {
                    return true;
                }
            }
        }

        // Allow CSS variables and calc expressions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        return false;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("columns", value, important));
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "columns: [value]";
}