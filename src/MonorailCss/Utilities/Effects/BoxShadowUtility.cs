using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling the box shadow of an element.
/// </summary>
internal class BoxShadowUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["shadow"];

    protected override string[] ThemeKeys => [];

    protected override string DefaultValue => "0 1px 3px 0 var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 1px 2px -1px var(--tw-shadow-color, rgb(0 0 0 / 0.1))";

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("--tw-shadow", value, important),
            new Declaration("box-shadow", "var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)", important));
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        if (isNegative)
        {
            return false;
        }

        // Handle arbitrary values
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            if (IsValidBoxShadowValue(arbitrary))
            {
                resolvedValue = arbitrary;
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Use direct shadow value mapping to match Tailwind exactly
            resolvedValue = key switch
            {
                "none" => "0 0 #0000",
                "inner" => "inset 0 2px 4px 0 var(--tw-shadow-color, rgb(0 0 0 / 0.05))",
                "sm" => "0 1px 3px 0 var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 1px 2px -1px var(--tw-shadow-color, rgb(0 0 0 / 0.1))",
                "md" => "0 4px 6px -1px var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 2px 4px -2px var(--tw-shadow-color, rgb(0 0 0 / 0.1))",
                "lg" => "0 10px 15px -3px var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 4px 6px -4px var(--tw-shadow-color, rgb(0 0 0 / 0.1))",
                "xl" => "0 20px 25px -5px var(--tw-shadow-color, rgb(0 0 0 / 0.1)), 0 8px 10px -6px var(--tw-shadow-color, rgb(0 0 0 / 0.1))",
                "2xl" => "0 25px 50px -12px var(--tw-shadow-color, rgb(0 0 0 / 0.25))",
                _ => string.Empty,
            };

            return !string.IsNullOrEmpty(resolvedValue);
        }

        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid box-shadow value.
    /// </summary>
    private static bool IsValidBoxShadowValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "none", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Basic validation for box-shadow syntax
        // This could be more comprehensive but covers common cases
        var trimmed = value.Trim();

        // Allow inset keyword
        if (trimmed.StartsWith("inset "))
        {
            return true;
        }

        // Allow color values (hex, rgb, hsl, etc.)
        if (trimmed.StartsWith("#") ||
            trimmed.StartsWith("rgb") ||
            trimmed.StartsWith("hsl") ||
            trimmed.StartsWith("color"))
        {
            return true;
        }

        // Allow numeric values (offset-x, offset-y, blur-radius, spread-radius)
        if (char.IsDigit(trimmed[0]) || trimmed[0] == '-')
        {
            return true;
        }

        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidBoxShadowValue(value);
    }
}