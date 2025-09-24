using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Filters;

/// <summary>
/// Utility for invert filter values.
/// Handles: invert, invert-0, invert-*
/// CSS: --tw-invert and filter property with CSS variable system.
/// </summary>
internal class InvertUtility : BaseFilterUtility
{
    protected override string[] Patterns => ["invert"];
    protected override string[] ThemeKeys => NamespaceResolver.InvertChain;
    protected override string FilterVariableName => "invert";
    protected override string DefaultValue => "invert(100%)";

    /// <summary>
    /// Static invert mappings for built-in invert values.
    /// </summary>
    private static readonly ImmutableDictionary<string, string> _staticInvertValues =
        new Dictionary<string, string>
        {
            ["0"] = "invert(0%)",
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

            if (IsValidInvertValue(arbitrary))
            {
                // Wrap in invert() function if not already wrapped
                resolvedValue = arbitrary.StartsWith("invert(") ? arbitrary : $"invert({arbitrary})";
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Check static values first
            if (_staticInvertValues.TryGetValue(key, out var staticValue))
            {
                resolvedValue = staticValue;
                return true;
            }

            // Convert numeric values to percentages
            if (int.TryParse(key, out var numericValue))
            {
                // Keep as percentage (e.g., 100 -> 100%, 50 -> 50%, 0 -> 0%)
                resolvedValue = $"invert({numericValue}%)";
                return true;
            }

            // Try theme resolution for other values
            var themeValue = theme.Resolve(key, ThemeKeys);
            if (!string.IsNullOrEmpty(themeValue))
            {
                resolvedValue = $"invert({themeValue})";
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid invert value.
    /// </summary>
    private static bool IsValidInvertValue(string value)
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
        if (value.StartsWith("var(") || value.Contains("calc(") || value.StartsWith("invert("))
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
        return IsValidInvertValue(value);
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register CSS variables for filter
        propertyRegistry.Register("--tw-blur", "*", false, string.Empty);
        propertyRegistry.Register("--tw-brightness", "*", false, string.Empty);
        propertyRegistry.Register("--tw-contrast", "*", false, string.Empty);
        propertyRegistry.Register("--tw-grayscale", "*", false, string.Empty);
        propertyRegistry.Register("--tw-hue-rotate", "*", false, string.Empty);
        propertyRegistry.Register("--tw-invert", "*", false, string.Empty);
        propertyRegistry.Register("--tw-saturate", "*", false, string.Empty);
        propertyRegistry.Register("--tw-sepia", "*", false, string.Empty);
        propertyRegistry.Register("--tw-drop-shadow", "*", false, string.Empty);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}