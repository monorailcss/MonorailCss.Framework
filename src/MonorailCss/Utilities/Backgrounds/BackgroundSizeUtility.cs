using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utility for background-size values.
/// Handles: bg-auto, bg-cover, bg-contain, bg-[length:200px_100px]
/// CSS: background-size: auto, background-size: cover, background-size: contain, background-size: 200px 100px.
/// </summary>
internal class BackgroundSizeUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["bg"];

    protected override string[] ThemeKeys => [];

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("background-size", value, important));
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle static values first
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Handle background-size keyword values
            resolvedValue = key switch
            {
                "auto" => "auto",
                "cover" => "cover",
                "contain" => "contain",
                _ => string.Empty,
            };

            if (!string.Empty.Equals(resolvedValue))
            {
                return true;
            }
        }

        // Handle arbitrary values with length: prefix (bg-[length:200px_100px])
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Check for length: prefix which indicates background-size
            if (arbitrary.StartsWith("length:"))
            {
                var lengthValue = arbitrary[7..]; // Remove "length:" prefix

                // Convert underscores to spaces (200px_100px -> 200px 100px)
                var processedValue = lengthValue.Replace("_", " ");

                if (IsValidBackgroundSizeValue(processedValue))
                {
                    resolvedValue = processedValue;
                    return true;
                }
            }

            // For other arbitrary values, don't handle them here - they might be for position
        }

        // No theme resolution needed for background-size
        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid background-size value.
    /// </summary>
    private static bool IsValidBackgroundSizeValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "auto", "cover", "contain", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow length values and percentage values
        var validUnits = new[] { "px", "em", "rem", "ch", "ex", "%", "vw", "vh", "vmin", "vmax" };
        var parts = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Background-size can have 1 or 2 values
        if (parts.Length is 0 or > 2)
        {
            return false;
        }

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
        // Only handle arbitrary values with length: prefix
        if (value.StartsWith("length:"))
        {
            var lengthValue = value[7..].Replace("_", " ");
            return IsValidBackgroundSizeValue(lengthValue);
        }

        return false;
    }

    // Lower priority since this handles fewer cases than BackgroundImageUtility
    public override UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;
}