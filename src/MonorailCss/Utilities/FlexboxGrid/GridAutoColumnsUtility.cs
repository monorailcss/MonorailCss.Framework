using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling the size of implicitly-created grid columns.
/// </summary>
internal class GridAutoColumnsUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["auto-cols"];

    protected override string[] ThemeKeys => [];

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("grid-auto-columns", value, important));
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
                "auto" => "auto",
                "min" => "min-content",
                "max" => "max-content",
                "fr" => "minmax(0, 1fr)",
                _ => string.Empty,
            };

            if (!string.Empty.Equals(resolvedValue))
            {
                return true;
            }
        }

        // Handle arbitrary values (auto-cols-[200px])
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Replace underscores with spaces and handle special character escaping
            var gridValue = arbitrary.Replace('_', ' ');

            // Basic validation - should be valid grid auto columns syntax
            if (IsValidGridAutoValue(gridValue))
            {
                resolvedValue = gridValue;
                return true;
            }
        }

        // No theme resolution needed for grid-auto-columns
        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid grid-auto-columns value.
    /// </summary>
    private static bool IsValidGridAutoValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "auto", "min-content", "max-content", "fit-content", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc(") ||
            value.Contains("minmax(") || value.Contains("min(") || value.Contains("max(") ||
            value.Contains("fit-content("))
        {
            return true;
        }

        // Allow length values, percentages, fr units
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
        return IsValidGridAutoValue(value.Replace('_', ' '));
    }

    public string[] GetDocumentedProperties() => ["grid-auto-columns"];

    public override IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return
        [
            new Documentation.UtilityExample("auto-cols-auto", "Set grid auto columns to auto"),
            new Documentation.UtilityExample("auto-cols-min", "Set grid auto columns to min-content"),
            new Documentation.UtilityExample("auto-cols-max", "Set grid auto columns to max-content"),
            new Documentation.UtilityExample("auto-cols-fr", "Set grid auto columns to minmax(0, 1fr)"),
            new Documentation.UtilityExample("auto-cols-(--my-size)", "Use a custom property for grid auto columns"),
            new Documentation.UtilityExample("auto-cols-[200px]", "Use an arbitrary value for grid auto columns"),
        ];
    }
}