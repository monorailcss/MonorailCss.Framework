using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utilities for controlling the position of background images.
/// </summary>
internal class BackgroundPositionUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["bg"];

    protected override string[] ThemeKeys => [];

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("background-position", value, important));
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle static values first
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Handle background-position keyword values
            resolvedValue = key switch
            {
                // Basic positions
                "top" => "top",
                "center" => "center",
                "bottom" => "bottom",
                "left" => "left",
                "right" => "right",

                // Corner positions (use two-value syntax)
                // Both orderings for compatibility
                "left-top" => "left top",
                "left-bottom" => "left bottom",
                "right-top" => "right top",
                "right-bottom" => "right bottom",
                "top-left" => "left top",
                "top-right" => "right top",
                "bottom-left" => "left bottom",
                "bottom-right" => "right bottom",
                _ => string.Empty,
            };

            if (!string.Empty.Equals(resolvedValue))
            {
                return true;
            }
        }

        // Handle arbitrary values (bg-[50%], bg-[100px], bg-[center_top])
        if (value.Kind == ValueKind.Arbitrary)
        {
            var hint = value.DataTypeHint;

            // If the candidate carries a non-position hint, this isn't ours.
            // Tailwind's `bg-[size:100%]` and `bg-[color:var(--x)]` would
            // otherwise look identical to a position checker.
            if (hint != null && hint != "position" && hint != "percentage")
            {
                return false;
            }

            var arbitrary = value.Value;
            var processedValue = arbitrary.Replace("_", " ");

            // Without an explicit position hint, bare `var()` / `calc()` could
            // be anything — defer to BackgroundColorUtility (the namespace
            // default for `bg-*`). Only claim values that are obviously a
            // position: keywords or unit-bearing literals.
            if (hint == null && !IsObviousBackgroundPositionValue(processedValue))
            {
                return false;
            }

            if (IsValidBackgroundPositionValue(processedValue))
            {
                resolvedValue = processedValue;
                return true;
            }
        }

        // No theme resolution needed for background-position
        return false;
    }

    // Stricter than IsValidBackgroundPositionValue: rejects var() and calc()
    // because those values can hold any type and shouldn't auto-claim the
    // position route without an explicit hint.
    private static bool IsObviousBackgroundPositionValue(string value)
    {
        if (value.StartsWith("var(", StringComparison.Ordinal) || value.Contains("calc("))
        {
            return false;
        }

        return IsValidBackgroundPositionValue(value);
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid background-position value.
    /// </summary>
    private static bool IsValidBackgroundPositionValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[]
        {
            "top", "center", "bottom", "left", "right",
            "inherit", "initial", "unset", "revert",
        };

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Split value by spaces to handle multi-value positions
        var parts = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Background-position can have 1 or 2 values
        if (parts.Length is 0 or > 2)
        {
            return false;
        }

        var validUnits = new[] { "px", "em", "rem", "ch", "ex", "%", "vw", "vh", "vmin", "vmax" };

        foreach (var part in parts)
        {
            var hasValidUnit = validUnits.Any(unit => part.EndsWith(unit));
            var isZero = part == "0";
            var isKeyword = keywords.Contains(part);

            if (!hasValidUnit && !isZero && !isKeyword)
            {
                return false;
            }
        }

        return true;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        var processedValue = value.Replace("_", " ");
        return IsValidBackgroundPositionValue(processedValue);
    }

    // Lower priority since this handles arbitrary values that might conflict with other bg-* utilities
    public override UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    protected override string GetSampleCssForArbitraryValue(string pattern) => "background-position: [value]";
}