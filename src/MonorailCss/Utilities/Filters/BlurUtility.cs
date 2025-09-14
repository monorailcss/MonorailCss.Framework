using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Filters;

/// <summary>
/// Utility for blur filter values.
/// Handles: blur-none, blur-sm, blur, blur-md, blur-lg, blur-xl, blur-2xl, blur-3xl, blur-*
/// CSS: --tw-blur and filter property with CSS variable system.
/// </summary>
internal class BlurUtility : BaseFilterUtility
{
    protected override string[] Patterns => ["blur"];
    protected override string[] ThemeKeys => NamespaceResolver.BlurChain;
    protected override string FilterVariableName => "blur";
    protected override string DefaultValue => "blur(4px)";

    /// <summary>
    /// Static blur mappings for built-in blur values.
    /// </summary>
    private static readonly ImmutableDictionary<string, string> _staticBlurValues =
        new Dictionary<string, string>
        {
            ["none"] = string.Empty,
            ["sm"] = "blur(4px)",
            ["md"] = "blur(12px)",
            ["lg"] = "blur(16px)",
            ["xl"] = "blur(24px)",
            ["2xl"] = "blur(40px)",
            ["3xl"] = "blur(64px)",
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

            if (IsValidBlurValue(arbitrary))
            {
                // Wrap in blur() function if not already wrapped
                resolvedValue = arbitrary.StartsWith("blur(") ? arbitrary : $"blur({arbitrary})";
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Check static values first
            if (_staticBlurValues.TryGetValue(key, out var staticValue))
            {
                resolvedValue = staticValue;
                return true;
            }

            // Try theme resolution for other values
            var themeValue = theme.Resolve(key, ThemeKeys);
            if (!string.IsNullOrEmpty(themeValue))
            {
                resolvedValue = $"blur({themeValue})";
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid blur value.
    /// </summary>
    private static bool IsValidBlurValue(string value)
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
        if (value.StartsWith("var(") || value.Contains("calc(") || value.StartsWith("blur("))
        {
            return true;
        }

        // Allow numeric values with length units (px, rem, em, etc.)
        if (IsValidLengthValue(value))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Validates if the value is a valid CSS length value.
    /// </summary>
    private static bool IsValidLengthValue(string value)
    {
        value = value.Trim();

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        // Allow 0 without unit
        if (value == "0")
        {
            return true;
        }

        // Check for numeric values with units
        var units = new[] { "px", "rem", "em", "ex", "ch", "vw", "vh", "vmin", "vmax", "cm", "mm", "in", "pt", "pc" };

        foreach (var unit in units)
        {
            if (value.EndsWith(unit))
            {
                var numPart = value[..^unit.Length];
                if (float.TryParse(numPart, out _))
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidBlurValue(value);
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