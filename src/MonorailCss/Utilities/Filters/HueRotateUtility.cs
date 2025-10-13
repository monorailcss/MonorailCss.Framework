using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Filters;

/// <summary>
/// Utilities for controlling the hue rotation filter of an element.
/// </summary>
internal class HueRotateUtility : BaseFilterUtility
{
    protected override string[] Patterns => ["hue-rotate"];
    protected override string[] ThemeKeys => NamespaceResolver.HueRotateChain;
    protected override string FilterVariableName => "hue-rotate";
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
                var finalValue = arbitrary.StartsWith("hue-rotate(") ? arbitrary : $"hue-rotate({arbitrary})";

                // Apply negative calculation if needed
                if (isNegative)
                {
                    finalValue = ApplyNegative(finalValue);
                }

                resolvedValue = finalValue;
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
                var degreeValue = $"{numericValue}deg";
                var finalValue = $"hue-rotate({degreeValue})";

                // Apply negative calculation if needed
                if (isNegative)
                {
                    finalValue = ApplyNegative(finalValue);
                }

                resolvedValue = finalValue;
                return true;
            }

            // Try theme resolution for other values
            var themeValue = theme.Resolve(key, ThemeKeys);
            if (!string.IsNullOrEmpty(themeValue))
            {
                var finalValue = $"hue-rotate({themeValue})";

                // Apply negative calculation if needed
                if (isNegative)
                {
                    finalValue = ApplyNegative(finalValue);
                }

                resolvedValue = finalValue;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Applies negative value transformation to hue-rotate values.
    /// </summary>
    private static string ApplyNegative(string hueRotateValue)
    {
        // If the value is already wrapped in hue-rotate(), extract the inner value
        if (hueRotateValue.StartsWith("hue-rotate(") && hueRotateValue.EndsWith(")"))
        {
            var innerValue = hueRotateValue[11..^1]; // Remove "hue-rotate(" and ")"
            return $"hue-rotate(calc({innerValue} * -1))";
        }

        // For bare values, wrap in calc
        return $"hue-rotate(calc({hueRotateValue} * -1))";
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

        // Allow angle values (degrees, radians, turns, etc.)
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
        var angleUnits = new[] { "deg", "grad", "rad", "turn" };

        foreach (var unit in angleUnits)
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

        // Also allow bare numeric values (will be treated as degrees)
        return float.TryParse(value, out _);
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