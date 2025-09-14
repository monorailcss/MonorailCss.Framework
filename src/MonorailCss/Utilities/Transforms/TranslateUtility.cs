using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utility for translate transform values.
/// Handles: translate-x-*, translate-y-*, -translate-x-*, -translate-y-*
/// CSS: Uses modern translate property with CSS variables --tw-translate-x and --tw-translate-y.
/// </summary>
internal class TranslateUtility : BaseSpacingUtility
{
    protected override string[] Patterns => ["translate-x", "translate-y"];
    protected override string[] SpacingNamespaces => NamespaceResolver.TranslateChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        // Handle percentage values for translate (full, 1/2, 1/3, etc.)
        var translatedValue = TranslatePercentageValue(value);

        return pattern switch
        {
            "translate-x" => ImmutableList.Create<AstNode>(
                new Declaration("--tw-translate-x", translatedValue, important),
                new Declaration("translate", "var(--tw-translate-x) var(--tw-translate-y)", important)),
            "translate-y" => ImmutableList.Create<AstNode>(
                new Declaration("--tw-translate-y", translatedValue, important),
                new Declaration("translate", "var(--tw-translate-x) var(--tw-translate-y)", important)),
            _ => ImmutableList<AstNode>.Empty,
        };
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important, CssPropertyRegistry propertyRegistry)
    {
        // Register the CSS variables for translate
        propertyRegistry.Register("--tw-translate-x", "*", false, "0");
        propertyRegistry.Register("--tw-translate-y", "*", false, "0");

        // Call the base implementation
        return GenerateDeclarations(pattern, value, important);
    }

    /// <summary>
    /// Translates special percentage values like "full" and fractions to CSS values.
    /// </summary>
    private static string TranslatePercentageValue(string value)
    {
        // Handle "full" -> 100%
        if (value == "100%" || value.Contains("full"))
        {
            return "100%";
        }

        // Keep Tailwind's calc format for fractions
        return value;
    }

    protected override bool TryResolveSpacing(CandidateValue value, Theme.Theme theme, out string spacing)
    {
        spacing = string.Empty;

        // Handle arbitrary values
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;
            if (ValueTypeInference.InferType(arbitrary) == ValueTypeInference.ValueType.Length ||
                IsValidPercentageValue(arbitrary))
            {
                // Negative values are handled by NegativeValueNormalizationStage
                spacing = arbitrary;
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Special handling for full -> 100%
            if (key == "full")
            {
                // Negative values are handled by NegativeValueNormalizationStage
                spacing = "100%";
                return true;
            }

            // Handle fractions (1/2, 1/3, 2/3, 1/4, etc.) - use Tailwind's calc format
            if (key.Contains('/'))
            {
                var parts = key.Split('/');
                if (parts.Length == 2 &&
                    double.TryParse(parts[0], out _) &&
                    double.TryParse(parts[1], out var denominator) &&
                    denominator != 0)
                {
                    // Negative values are handled by NegativeValueNormalizationStage
                    spacing = $"calc({key} * 100%)";
                    return true;
                }
            }

            // Use base spacing resolution for numeric and theme values
            return base.TryResolveSpacing(value, theme, out spacing);
        }

        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid percentage value.
    /// </summary>
    private static bool IsValidPercentageValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
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

        return false;
    }
}