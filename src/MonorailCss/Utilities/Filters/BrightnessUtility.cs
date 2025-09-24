using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Filters;

/// <summary>
/// Utility for brightness filter values.
/// Handles: brightness-0, brightness-50, brightness-75, brightness-90, brightness-95, brightness-100, brightness-105, brightness-110, brightness-125, brightness-150, brightness-200, brightness-*
/// CSS: --tw-brightness and filter property with CSS variable system.
/// </summary>
internal class BrightnessUtility : BaseFilterUtility
{
    protected override string[] Patterns => ["brightness"];
    protected override string[] ThemeKeys => NamespaceResolver.BrightnessChain;
    protected override string FilterVariableName => "brightness";

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

            if (IsValidBrightnessValue(arbitrary))
            {
                // Wrap in brightness() function if not already wrapped
                resolvedValue = arbitrary.StartsWith("brightness(") ? arbitrary : $"brightness({arbitrary})";
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Convert numeric values to percentages
            if (int.TryParse(key, out var numericValue))
            {
                // Use percentage format (e.g., 100 -> 100%, 150 -> 150%, 50 -> 50%)
                resolvedValue = $"brightness({numericValue}%)";
                return true;
            }

            // Try theme resolution for other values
            var themeValue = theme.Resolve(key, ThemeKeys);
            if (!string.IsNullOrEmpty(themeValue))
            {
                resolvedValue = $"brightness({themeValue})";
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid brightness value.
    /// </summary>
    private static bool IsValidBrightnessValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "none", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc(") || value.StartsWith("brightness("))
        {
            return true;
        }

        // Allow numeric values (decimals or percentages)
        if (IsValidNumericValue(value))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Validates if the value is a valid numeric value (decimal or percentage).
    /// </summary>
    private static bool IsValidNumericValue(string value)
    {
        value = value.Trim();

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        // Handle percentages
        if (value.EndsWith("%"))
        {
            var numPart = value[..^1];
            return float.TryParse(numPart, out _);
        }

        // Handle decimal numbers
        return float.TryParse(value, out _);
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidBrightnessValue(value);
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Use shared method to register filter variables
        RegisterFilterVariables(propertyRegistry);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}