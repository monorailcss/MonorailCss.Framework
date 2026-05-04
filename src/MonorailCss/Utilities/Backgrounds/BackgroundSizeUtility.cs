using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utilities for controlling the size of background images.
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

        // Handle arbitrary values. Tailwind dispatches `bg-[size:100%]`,
        // `bg-[bg-size:100%]`, and `bg-[length:200px_100px]` all to
        // `background-size`. The decoder converts underscores to spaces, so the
        // value reaches us already in the form `200px 100px`.
        if (value.Kind == ValueKind.Arbitrary)
        {
            var hint = value.DataTypeHint;
            if (hint is "size" or "bg-size" or "length" or "percentage")
            {
                if (IsValidBackgroundSizeValue(value.Value))
                {
                    resolvedValue = value.Value;
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
        // Hint is consumed in TryResolveValue via CandidateValue.DataTypeHint;
        // by the time we reach here the type hint has already been stripped from
        // the value. Anything that looks like a valid background-size literal is
        // potentially ours (TryResolveValue ultimately gates on the hint).
        return IsValidBackgroundSizeValue(value);
    }

    // Lower priority since this handles fewer cases than BackgroundImageUtility
    public override UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    protected override string GetSampleCssForArbitraryValue(string pattern) => "background-size: [value]";
}