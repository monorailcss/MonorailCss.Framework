using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utility for scale transform values.
/// Handles: scale-*, scale-x-*, scale-y-*, -scale-*, -scale-x-*, -scale-y-*
/// CSS: Uses modern scale property with CSS variables --tw-scale-x and --tw-scale-y.
/// </summary>
internal class ScaleUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["scale", "scale-x", "scale-y", "scale-z"];
    protected override string[] ThemeKeys => NamespaceResolver.ScaleChain;
    protected override bool SupportsNegative => true;

    /// <summary>
    /// Static scale mappings for built-in scale values (percentages).
    /// </summary>
    private static readonly ImmutableDictionary<string, string> _staticScaleValues =
        new Dictionary<string, string>
        {
            ["none"] = "none",
            ["3d"] = "var(--tw-scale-x) var(--tw-scale-y) var(--tw-scale-z)",
            ["0"] = "0%",
            ["50"] = "50%",
            ["75"] = "75%",
            ["90"] = "90%",
            ["95"] = "95%",
            ["100"] = "100%",
            ["105"] = "105%",
            ["110"] = "110%",
            ["125"] = "125%",
            ["150"] = "150%",
            ["200"] = "200%",
        }.ToImmutableDictionary();

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle arbitrary values
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            if (IsValidScaleValue(arbitrary))
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
            if (_staticScaleValues.TryGetValue(key, out var staticValue))
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
            "scale" => ImmutableList.Create<AstNode>(
                new Declaration("--tw-scale-x", value, important),
                new Declaration("--tw-scale-y", value, important),
                new Declaration("--tw-scale-z", value, important),
                new Declaration("scale", "var(--tw-scale-x) var(--tw-scale-y)", important)),
            "scale-x" => ImmutableList.Create<AstNode>(
                new Declaration("--tw-scale-x", value, important),
                new Declaration("scale", "var(--tw-scale-x) var(--tw-scale-y)", important)),
            "scale-y" => ImmutableList.Create<AstNode>(
                new Declaration("--tw-scale-y", value, important),
                new Declaration("scale", "var(--tw-scale-x) var(--tw-scale-y)", important)),
            "scale-z" => ImmutableList.Create<AstNode>(
                new Declaration("--tw-scale-z", value, important),
                new Declaration("scale", "var(--tw-scale-x) var(--tw-scale-y) var(--tw-scale-z)", important)),
            _ => ImmutableList<AstNode>.Empty,
        };
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid scale value.
    /// </summary>
    private static bool IsValidScaleValue(string value)
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

        // Allow numeric values with optional % or decimal numbers
        if (IsValidNumericScaleValue(value))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Validates if the value is a valid numeric scale value.
    /// </summary>
    private static bool IsValidNumericScaleValue(string value)
    {
        value = value.Trim();

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        // Allow percentages
        if (value.EndsWith("%"))
        {
            var numPart = value[..^1];
            if (double.TryParse(numPart, NumberStyles.Number,
                CultureInfo.InvariantCulture, out _))
            {
                return true;
            }
        }

        // Allow plain numbers (will be treated as decimal multipliers)
        if (double.TryParse(value, NumberStyles.Number,
            CultureInfo.InvariantCulture, out _))
        {
            return true;
        }

        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidScaleValue(value);
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register CSS variables for scale
        propertyRegistry.Register("--tw-scale-x", "*", false, "1");
        propertyRegistry.Register("--tw-scale-y", "*", false, "1");
        propertyRegistry.Register("--tw-scale-z", "*", false, "1");

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}