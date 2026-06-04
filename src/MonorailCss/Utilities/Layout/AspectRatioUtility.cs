using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling the aspect ratio of an element.
/// </summary>
internal class AspectRatioUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["aspect"];

    protected override string[] ThemeKeys => ["--aspect"];

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("aspect-ratio", value, important));
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle static values first
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Built-in keywords. `auto`/`square` are not theme-backed; other named values
            // (e.g. `video`) resolve through the --aspect-* namespace to `var(--aspect-<key>)`.
            switch (key)
            {
                case "auto":
                    resolvedValue = "auto";
                    return true;
                case "square":
                    resolvedValue = "1 / 1";
                    return true;
            }

            var themeValue = theme.Resolve(key, ThemeKeys);
            if (!string.IsNullOrEmpty(themeValue))
            {
                resolvedValue = themeValue;
                return true;
            }
        }

        // Handle arbitrary values (aspect-[16/9], aspect-[4/3])
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Check if it's a fraction format like "16/9", "4/3", etc.
            if (IsValidAspectRatio(arbitrary))
            {
                resolvedValue = arbitrary;
                return true;
            }

            // Allow decimals and convert them to fraction format if they make sense
            if (decimal.TryParse(arbitrary, out var decimalValue) && decimalValue > 0)
            {
                resolvedValue = decimalValue.ToString(CultureInfo.InvariantCulture);
                return true;
            }
        }

        // No theme resolution needed for aspect-ratio
        return false;
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid aspect-ratio value.
    /// </summary>
    private static bool IsValidAspectRatio(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "auto", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow fraction format like "16/9", "4/3", etc.
        if (value.Contains('/'))
        {
            var parts = value.Split('/');
            if (parts.Length == 2 &&
                decimal.TryParse(parts[0].Trim(), out var numerator) && numerator > 0 &&
                decimal.TryParse(parts[1].Trim(), out var denominator) && denominator > 0)
            {
                return true;
            }
        }

        // Allow single decimal/integer values
        if (decimal.TryParse(value, out var decimalValue) && decimalValue > 0)
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidAspectRatio(value);
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "aspect-ratio: [value]";
}