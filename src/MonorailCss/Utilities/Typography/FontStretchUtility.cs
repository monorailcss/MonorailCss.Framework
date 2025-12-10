using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the font stretch of an element.
/// </summary>
internal class FontStretchUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["font-stretch"];

    protected override string[] ThemeKeys => [];

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("font-stretch", value, important));
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle static values first
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Handle font-stretch keyword values
            resolvedValue = key switch
            {
                "normal" => "normal",
                "ultra-condensed" => "ultra-condensed",
                "extra-condensed" => "extra-condensed",
                "condensed" => "condensed",
                "semi-condensed" => "semi-condensed",
                "semi-expanded" => "semi-expanded",
                "expanded" => "expanded",
                "extra-expanded" => "extra-expanded",
                "ultra-expanded" => "ultra-expanded",
                _ => string.Empty,
            };

            if (!string.Empty.Equals(resolvedValue))
            {
                return true;
            }

            // Handle percentage values as named values (e.g., font-stretch-100%)
            if (key.EndsWith("%") && key.Length > 1)
            {
                var percentStr = key[..^1];
                if (float.TryParse(percentStr, out var percent) && percent >= 0)
                {
                    resolvedValue = key;
                    return true;
                }
            }
        }

        // Handle arbitrary values (font-stretch-[75%])
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Allow arbitrary percentage and keyword values
            if (IsValidFontStretchValue(arbitrary))
            {
                resolvedValue = arbitrary;
                return true;
            }
        }

        // No theme resolution needed for font-stretch
        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid font-stretch value.
    /// </summary>
    private static bool IsValidFontStretchValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[]
        {
            "normal", "ultra-condensed", "extra-condensed", "condensed",
            "semi-condensed", "semi-expanded", "expanded", "extra-expanded",
            "ultra-expanded", "inherit", "initial", "unset", "revert",
        };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow percentage values (50% - 200% typically)
        if (value.EndsWith("%") && value.Length > 1)
        {
            var percentStr = value[..^1];
            if (float.TryParse(percentStr, out var percent) && percent >= 0)
            {
                return true;
            }
        }

        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidFontStretchValue(value);
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "font-stretch: [value]";
}