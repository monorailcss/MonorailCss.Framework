using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the thickness of text decorations.
/// </summary>
internal class TextDecorationThicknessUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["decoration"];

    protected override string[] ThemeKeys => [];

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("text-decoration-thickness", value, important));
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
                "from-font" => "from-font",
                _ => string.Empty,
            };

            if (!string.Empty.Equals(resolvedValue))
            {
                return true;
            }

            // Handle numeric values (decoration-0, decoration-1, etc.)
            if (int.TryParse(key, out var numValue) && numValue >= 0)
            {
                resolvedValue = $"{numValue}px";
                return true;
            }
        }

        // Handle arbitrary values (decoration-[3px])
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Allow arbitrary length values
            if (IsValidThicknessValue(arbitrary))
            {
                resolvedValue = arbitrary;
                return true;
            }
        }

        // No theme resolution needed for text-decoration-thickness
        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid text-decoration-thickness value.
    /// </summary>
    private static bool IsValidThicknessValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "auto", "from-font", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow length values
        var validUnits = new[] { "px", "em", "rem", "ch", "ex", "%", "vw", "vh", "vmin", "vmax" };
        var hasValidUnit = validUnits.Any(unit => value.EndsWith(unit));
        var isZero = value == "0";

        if (!hasValidUnit && !isZero)
        {
            return false;
        }

        return true;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidThicknessValue(value);
    }

    // Override priority to handle potential conflicts with decoration color utilities
    // Note: This utility should have higher priority when both could match "decoration-*"
    public override UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;
}