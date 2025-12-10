using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling the text shadow of an element.
/// </summary>
internal class TextShadowUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["text-shadow"];

    protected override string[] ThemeKeys => NamespaceResolver.TextShadowChain;

    protected override string DefaultValue => "0px 1px 1px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1)), 0px 1px 2px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1)), 0px 2px 4px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1))";

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("text-shadow", value, important));
    }

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

            if (IsValidTextShadowValue(arbitrary))
            {
                resolvedValue = arbitrary;
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Use direct text-shadow value mapping to match Tailwind exactly
            resolvedValue = key switch
            {
                "none" => "none",
                "sm" => "0px 1px 0px var(--tw-text-shadow-color, rgb(0 0 0 / 0.075)), 0px 1px 1px var(--tw-text-shadow-color, rgb(0 0 0 / 0.075)), 0px 2px 2px var(--tw-text-shadow-color, rgb(0 0 0 / 0.075))",
                "md" => "0px 1px 1px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1)), 0px 1px 2px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1)), 0px 2px 4px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1))",
                "lg" => "0px 1px 2px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1)), 0px 3px 2px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1)), 0px 4px 8px var(--tw-text-shadow-color, rgb(0 0 0 / 0.1))",
                _ => string.Empty,
            };

            return !string.IsNullOrEmpty(resolvedValue);
        }

        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid text-shadow value.
    /// </summary>
    private static bool IsValidTextShadowValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "none", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Basic validation for text-shadow syntax
        // This could be more comprehensive but covers common cases
        var trimmed = value.Trim();

        // Allow color values (hex, rgb, hsl, etc.)
        if (trimmed.StartsWith("#") ||
            trimmed.StartsWith("rgb") ||
            trimmed.StartsWith("hsl") ||
            trimmed.StartsWith("color"))
        {
            return true;
        }

        // Allow numeric values (offset-x, offset-y, blur-radius)
        if (char.IsDigit(trimmed[0]) || trimmed[0] == '-')
        {
            return true;
        }

        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidTextShadowValue(value);
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "text-shadow: [value]";
}