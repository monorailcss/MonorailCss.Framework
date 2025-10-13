using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utilities for controlling the skew of an element.
/// </summary>
internal class SkewUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["skew", "skew-x", "skew-y"];
    protected override string[] ThemeKeys => NamespaceResolver.SkewChain;
    protected override bool SupportsNegative => true;

    /// <summary>
    /// Static skew mappings for built-in skew values (degrees).
    /// </summary>
    private static readonly ImmutableDictionary<string, string> _staticSkewValues =
        new Dictionary<string, string>
        {
            ["0"] = "0deg",
            ["1"] = "1deg",
            ["2"] = "2deg",
            ["3"] = "3deg",
            ["6"] = "6deg",
            ["12"] = "12deg",
        }.ToImmutableDictionary();

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle arbitrary values
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            if (IsValidSkewValue(arbitrary))
            {
                resolvedValue = isNegative ? $"calc({arbitrary} * -1)" : arbitrary;
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Check static values first
            if (_staticSkewValues.TryGetValue(key, out var staticValue))
            {
                resolvedValue = isNegative ? $"calc({staticValue} * -1)" : staticValue;
                return true;
            }

            // Try theme resolution for other values
            var themeValue = theme.Resolve(key, ThemeKeys);
            if (!string.IsNullOrEmpty(themeValue))
            {
                resolvedValue = isNegative ? $"calc({themeValue} * -1)" : themeValue;
                return true;
            }
        }

        return false;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return pattern switch
        {
            "skew" => ImmutableList.Create<AstNode>(
                new Declaration("--tw-skew-x", $"skewX({value})", important),
                new Declaration("--tw-skew-y", $"skewY({value})", important),
                new Declaration("transform", "var(--tw-rotate-x) var(--tw-rotate-y) var(--tw-rotate-z) var(--tw-skew-x) var(--tw-skew-y)", important)),
            "skew-x" => ImmutableList.Create<AstNode>(
                new Declaration("--tw-skew-x", $"skewX({value})", important),
                new Declaration("transform", "var(--tw-rotate-x) var(--tw-rotate-y) var(--tw-rotate-z) var(--tw-skew-x) var(--tw-skew-y)", important)),
            "skew-y" => ImmutableList.Create<AstNode>(
                new Declaration("--tw-skew-y", $"skewY({value})", important),
                new Declaration("transform", "var(--tw-rotate-x) var(--tw-rotate-y) var(--tw-rotate-z) var(--tw-skew-x) var(--tw-skew-y)", important)),
            _ => ImmutableList<AstNode>.Empty,
        };
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid skew value.
    /// </summary>
    private static bool IsValidSkewValue(string value)
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
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow angle values
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

        // Check for angle values with units
        var angleUnits = new[] { "deg", "rad", "grad", "turn" };

        foreach (var unit in angleUnits)
        {
            if (value.EndsWith(unit))
            {
                var numPart = value[..^unit.Length];
                if (double.TryParse(numPart, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out _))
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidSkewValue(value);
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register CSS variables for skew and rotate (used in transform)
        propertyRegistry.Register("--tw-rotate-x", "*", false, null);
        propertyRegistry.Register("--tw-rotate-y", "*", false, null);
        propertyRegistry.Register("--tw-rotate-z", "*", false, null);
        propertyRegistry.Register("--tw-skew-x", "*", false, null);
        propertyRegistry.Register("--tw-skew-y", "*", false, null);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}