using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Filters;

/// <summary>
/// Utilities for controlling the backdrop hue rotation filter of an element.
/// </summary>
internal class BackdropHueRotateUtility : BaseFilterUtility
{
    protected override string[] Patterns => ["backdrop-hue-rotate"];
    protected override string[] ThemeKeys => NamespaceResolver.BackdropHueRotateChain;
    protected override string FilterVariableName => "hue-rotate";
    protected override bool IsBackdropFilter => true;
    protected override bool SupportsNegative => true;

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle arbitrary values
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            if (IsValidHueRotateValue(arbitrary))
            {
                // Wrap in hue-rotate() function if not already wrapped
                var hueValue = arbitrary.StartsWith("hue-rotate(") ? arbitrary : $"hue-rotate({arbitrary})";
                resolvedValue = isNegative ? $"hue-rotate(-{ExtractValue(hueValue)})" : hueValue;
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Convert numeric values to degrees
            if (int.TryParse(key, out var numericValue))
            {
                var degrees = isNegative ? -numericValue : numericValue;
                resolvedValue = $"hue-rotate({degrees}deg)";
                return true;
            }

            // Try theme resolution for other values
            var themeValue = theme.Resolve(key, ThemeKeys);
            if (!string.IsNullOrEmpty(themeValue))
            {
                var hueValue = themeValue.StartsWith("hue-rotate(") ? themeValue : $"hue-rotate({themeValue})";
                resolvedValue = isNegative ? $"hue-rotate(-{ExtractValue(hueValue)})" : hueValue;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Extracts the value from a hue-rotate function (e.g., "hue-rotate(45deg)" -> "45deg").
    /// </summary>
    private static string ExtractValue(string hueRotateFunction)
    {
        if (hueRotateFunction.StartsWith("hue-rotate(") && hueRotateFunction.EndsWith(")"))
        {
            return hueRotateFunction[11..^1]; // Remove "hue-rotate(" and ")"
        }

        return hueRotateFunction;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid hue-rotate value.
    /// </summary>
    private static bool IsValidHueRotateValue(string value)
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
        if (value.StartsWith("var(") || value.Contains("calc(") || value.StartsWith("hue-rotate("))
        {
            return true;
        }

        // Allow angle values (deg, rad, grad, turn)
        if (IsValidAngleValue(value))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Validates if the value is a valid CSS angle value.
    /// </summary>
    private static bool IsValidAngleValue(string value)
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

        // Check for numeric values with angle units
        var units = new[] { "deg", "rad", "grad", "turn" };

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
        return IsValidHueRotateValue(value);
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Use shared method to register filter variables
        RegisterFilterVariables(propertyRegistry);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}