using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for specifying the rows in a grid layout.
/// </summary>
internal class GridTemplateRowsUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["grid-rows"];

    protected override string[] ThemeKeys => NamespaceResolver.GridTemplateRowsChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("grid-template-rows", value, important));
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle static values first
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Handle special static values
            resolvedValue = key switch
            {
                "none" => "none",
                "subgrid" => "subgrid",
                _ => string.Empty,
            };

            if (!string.Empty.Equals(resolvedValue))
            {
                return true;
            }

            // Handle numeric values (grid-rows-1, grid-rows-2, etc.)
            if (int.TryParse(key, out var numValue) && numValue >= 1 && numValue <= 12)
            {
                resolvedValue = $"repeat({numValue}, minmax(0, 1fr))";
                return true;
            }
        }

        // Handle arbitrary values (grid-rows-[200px_minmax(900px,_1fr)_100px])
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Replace underscores with spaces and handle special character escaping
            var gridValue = arbitrary.Replace('_', ' ');

            // Basic validation - should be valid grid template syntax
            if (IsValidGridTemplateValue(gridValue))
            {
                resolvedValue = gridValue;
                return true;
            }
        }

        // Try theme resolution as fallback
        return base.TryResolveValue(value, theme, isNegative, out resolvedValue);
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid grid-template-rows value.
    /// </summary>
    private static bool IsValidGridTemplateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "none", "subgrid", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc(") || value.Contains("repeat(") ||
            value.Contains("minmax(") || value.Contains("min(") || value.Contains("max("))
        {
            return true;
        }

        // Allow length values, percentages, fr units, and combinations
        var validUnits = new[] { "px", "em", "rem", "ch", "vw", "vh", "vmin", "vmax", "%", "fr" };
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            // Skip function calls and keywords
            if (part.Contains('(') || keywords.Contains(part))
            {
                continue;
            }

            // Check if it ends with a valid unit or is a number
            var hasValidUnit = validUnits.Any(unit => part.EndsWith(unit));
            var isNumber = double.TryParse(part, out _);

            if (!hasValidUnit && !isNumber)
            {
                return false;
            }
        }

        return true;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidGridTemplateValue(value.Replace('_', ' '));
    }
}