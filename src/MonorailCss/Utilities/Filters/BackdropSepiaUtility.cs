using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Filters;

/// <summary>
/// Utility for backdrop sepia filter values.
/// Handles: backdrop-sepia, backdrop-sepia-0, backdrop-sepia-*
/// CSS: --tw-backdrop-sepia and backdrop-filter property with CSS variable system.
/// </summary>
internal class BackdropSepiaUtility : BaseFilterUtility
{
    protected override string[] Patterns => ["backdrop-sepia"];
    protected override string[] ThemeKeys => NamespaceResolver.BackdropSepiaChain;
    protected override string FilterVariableName => "sepia";
    protected override string DefaultValue => "sepia(100%)";
    protected override bool IsBackdropFilter => true;

    /// <summary>
    /// Static sepia mappings for built-in sepia values.
    /// </summary>
    private static readonly ImmutableDictionary<string, string> _staticSepiaValues =
        new Dictionary<string, string>
        {
            ["0"] = "sepia(0%)",
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

            if (IsValidSepiaValue(arbitrary))
            {
                // Wrap in sepia() function if not already wrapped
                resolvedValue = arbitrary.StartsWith("sepia(") ? arbitrary : $"sepia({arbitrary})";
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Check static values first
            if (_staticSepiaValues.TryGetValue(key, out var staticValue))
            {
                resolvedValue = staticValue;
                return true;
            }

            // Convert numeric values to percentages
            if (int.TryParse(key, out var numericValue))
            {
                // Convert to percentage (e.g., 100 -> 100%, 50 -> 50%, 0 -> 0%)
                resolvedValue = $"sepia({numericValue}%)";
                return true;
            }

            // Try theme resolution for other values
            var themeValue = theme.Resolve(key, ThemeKeys);
            if (!string.IsNullOrEmpty(themeValue))
            {
                resolvedValue = $"sepia({themeValue})";
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid sepia value.
    /// </summary>
    private static bool IsValidSepiaValue(string value)
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
        if (value.StartsWith("var(") || value.Contains("calc(") || value.StartsWith("sepia("))
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
        return IsValidSepiaValue(value);
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
        propertyRegistry.Register("--tw-backdrop-saturate", "*", false, string.Empty);
        propertyRegistry.Register("--tw-backdrop-sepia", "*", false, string.Empty);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}