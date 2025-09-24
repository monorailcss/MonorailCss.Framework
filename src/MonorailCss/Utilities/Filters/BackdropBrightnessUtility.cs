using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Filters;

/// <summary>
/// Utility for backdrop brightness filter values.
/// Handles: backdrop-brightness-0, backdrop-brightness-50, backdrop-brightness-75, backdrop-brightness-90, backdrop-brightness-95, backdrop-brightness-100, backdrop-brightness-105, backdrop-brightness-110, backdrop-brightness-125, backdrop-brightness-150, backdrop-brightness-200, backdrop-brightness-*
/// CSS: --tw-backdrop-brightness and backdrop-filter property with CSS variable system.
/// </summary>
internal class BackdropBrightnessUtility : BaseFilterUtility
{
    protected override string[] Patterns => ["backdrop-brightness"];
    protected override string[] ThemeKeys => NamespaceResolver.BackdropBrightnessChain;
    protected override string FilterVariableName => "brightness";
    protected override bool IsBackdropFilter => true;

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

            // Convert numeric values to percentages or decimals
            if (int.TryParse(key, out var numericValue))
            {
                // Convert to percentage (e.g., 100 -> 100%, 150 -> 150%, 50 -> 50%)
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
        // Register CSS variables for backdrop filter
        propertyRegistry.Register("--tw-backdrop-blur", "*", false, string.Empty);
        propertyRegistry.Register("--tw-backdrop-brightness", "*", false, string.Empty);
        propertyRegistry.Register("--tw-backdrop-contrast", "*", false, string.Empty);
        propertyRegistry.Register("--tw-backdrop-grayscale", "*", false, string.Empty);
        propertyRegistry.Register("--tw-backdrop-hue-rotate", "*", false, string.Empty);
        propertyRegistry.Register("--tw-backdrop-invert", "*", false, string.Empty);
        propertyRegistry.Register("--tw-backdrop-opacity", "*", false, string.Empty);
        propertyRegistry.Register("--tw-backdrop-saturate", "*", false, string.Empty);
        propertyRegistry.Register("--tw-backdrop-sepia", "*", false, string.Empty);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}