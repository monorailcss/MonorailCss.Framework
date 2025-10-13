using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the leading (line height) of an element.
/// </summary>
internal class LineHeightUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["leading"];
    protected override string[] ThemeKeys => NamespaceResolver.LineHeightChain;

    /// <summary>
    /// Handles bare numeric values and named line-height values.
    /// Examples: "4" -> "1rem", "none" -> "1", "tight" -> "1.25".
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Handle named values
        var namedValues = new Dictionary<string, string>
        {
            ["none"] = "1",
            ["tight"] = "1.25",
            ["snug"] = "1.375",
            ["normal"] = "1.5",
            ["relaxed"] = "1.625",
            ["loose"] = "2",
            ["px"] = "1px",
        };

        if (namedValues.TryGetValue(value, out var namedValue))
        {
            return namedValue;
        }

        // Handle numeric values - these typically map to rem values for spacing
        if (int.TryParse(value, out var numValue) && numValue >= 0)
        {
            // For line-height, numeric values like "4" typically mean "1rem" (4 * 0.25rem)
            // But for line-height, we want unitless values for most cases
            // Let's check if this should be a rem value or unitless
            if (numValue <= 10)
            {
                // Small numbers are typically unitless multipliers
                return numValue.ToString();
            }

            // Larger numbers might be pixel/rem values
            // This would need to be handled by theme resolution
            return null;
        }

        // Handle decimal values (unitless line-height multipliers)
        if (double.TryParse(value, NumberStyles.Number,
            CultureInfo.InvariantCulture, out var decValue) && decValue > 0)
        {
            return decValue.ToString("G", CultureInfo.InvariantCulture);
        }

        return null;
    }

    /// <summary>
    /// Validates arbitrary values for line-height.
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        // Allow numeric values (unitless multipliers)
        if (double.TryParse(value, out var numValue) && numValue > 0)
        {
            return true;
        }

        // Allow values with units
        if (value.EndsWith("rem") || value.EndsWith("em") || value.EndsWith("px") ||
            value.EndsWith("%") || value.EndsWith("vh"))
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
        // Also set --tw-leading variable
        return ImmutableList.Create<AstNode>(
            new Declaration("--tw-leading", value, important),
            new Declaration("line-height", value, important));
    }
}