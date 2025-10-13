using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the tracking (letter spacing) of an element.
/// </summary>
internal class LetterSpacingUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["tracking"];
    protected override string[] ThemeKeys => NamespaceResolver.LetterSpacingChain;

    /// <summary>
    /// Handles named letter-spacing values.
    /// Examples: "tight" -> "-0.025em", "wide" -> "0.025em".
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Handle named values
        var namedValues = new Dictionary<string, string>
        {
            ["tighter"] = "-0.05em",
            ["tight"] = "-0.025em",
            ["normal"] = "0em",
            ["wide"] = "0.025em",
            ["wider"] = "0.05em",
            ["widest"] = "0.1em",
        };

        if (namedValues.TryGetValue(value, out var namedValue))
        {
            return namedValue;
        }

        // Handle numeric values - could be used for arbitrary tracking values
        if (double.TryParse(value, NumberStyles.Number,
            CultureInfo.InvariantCulture, out var numValue))
        {
            // Numeric values without units are typically treated as em values
            return $"{numValue}em";
        }

        return null;
    }

    /// <summary>
    /// Validates arbitrary values for letter-spacing.
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        // Allow numeric values (will be treated as em)
        if (double.TryParse(value, out _))
        {
            return true;
        }

        // Allow values with units
        if (value.EndsWith("em") || value.EndsWith("rem") || value.EndsWith("px") ||
            value.EndsWith("%") || value.EndsWith("ch"))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow keywords
        if (value is "normal" or "inherit" or "initial" or "unset")
        {
            return true;
        }

        return false;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("letter-spacing", value, important));
    }
}