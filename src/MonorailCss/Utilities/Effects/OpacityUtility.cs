using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utility for opacity values.
/// Handles: opacity-0, opacity-25, opacity-50, opacity-75, opacity-100, etc.
/// CSS: opacity: 0, opacity: 0.25, opacity: 0.5, opacity: 0.75, opacity: 1.
/// </summary>
internal class OpacityUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["opacity"];
    protected override string[] ThemeKeys => NamespaceResolver.OpacityChain;

    /// <summary>
    /// Handles bare numeric values by converting them to decimal opacity values.
    /// Examples: "0" -> "0", "50" -> "0.5", "100" -> "1".
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Try to parse as integer (0-100)
        if (int.TryParse(value, out var intValue) && intValue >= 0 && intValue <= 100)
        {
            // Convert percentage to decimal (0-100 -> 0.0-1.0)
            var decimalValue = intValue / 100.0;
            return decimalValue.ToString("G", CultureInfo.InvariantCulture);
        }

        return null;
    }

    /// <summary>
    /// Validates arbitrary values for opacity (should be between 0 and 1, or percentage).
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        // Allow decimal values between 0 and 1
        if (double.TryParse(value, out var decimalValue) && decimalValue >= 0 && decimalValue <= 1)
        {
            return true;
        }

        // Allow percentage values
        if (value.EndsWith('%') &&
            double.TryParse(value.TrimEnd('%'), out var percentValue) &&
            percentValue >= 0 && percentValue <= 100)
        {
            return true;
        }

        // Allow CSS variables and other valid CSS values
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        return false;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("opacity", value, important));
    }
}