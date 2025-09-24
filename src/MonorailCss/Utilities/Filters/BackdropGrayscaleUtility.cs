using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Filters;

/// <summary>
/// Utility for backdrop grayscale filter values.
/// Handles: backdrop-grayscale, backdrop-grayscale-0, backdrop-grayscale-*
/// CSS: --tw-backdrop-grayscale and backdrop-filter property with CSS variable system.
/// </summary>
internal class BackdropGrayscaleUtility : BaseFilterUtility
{
    protected override string[] Patterns => ["backdrop-grayscale"];
    protected override string[] ThemeKeys => NamespaceResolver.BackdropGrayscaleChain;
    protected override string FilterVariableName => "grayscale";
    protected override string DefaultValue => "grayscale(100%)";
    protected override bool IsBackdropFilter => true;

    /// <summary>
    /// Static grayscale mappings for built-in grayscale values.
    /// </summary>
    private static readonly ImmutableDictionary<string, string> _staticGrayscaleValues =
        new Dictionary<string, string>
        {
            ["0"] = "grayscale(0%)",
        }.ToImmutableDictionary();

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

            if (IsValidGrayscaleValue(arbitrary))
            {
                // Wrap in grayscale() function if not already wrapped
                resolvedValue = arbitrary.StartsWith("grayscale(") ? arbitrary : $"grayscale({arbitrary})";
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Check static values first
            if (_staticGrayscaleValues.TryGetValue(key, out var staticValue))
            {
                resolvedValue = staticValue;
                return true;
            }

            // Convert numeric values to percentages
            if (int.TryParse(key, out var numericValue))
            {
                // Convert to percentage (e.g., 100 -> 100%, 50 -> 50%, 0 -> 0%)
                resolvedValue = $"grayscale({numericValue}%)";
                return true;
            }

            // Try theme resolution for other values
            var themeValue = theme.Resolve(key, ThemeKeys);
            if (!string.IsNullOrEmpty(themeValue))
            {
                resolvedValue = $"grayscale({themeValue})";
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid grayscale value.
    /// </summary>
    private static bool IsValidGrayscaleValue(string value)
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
        if (value.StartsWith("var(") || value.Contains("calc(") || value.StartsWith("grayscale("))
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
        return IsValidGrayscaleValue(value);
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Use shared method to register filter variables
        RegisterFilterVariables(propertyRegistry);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}