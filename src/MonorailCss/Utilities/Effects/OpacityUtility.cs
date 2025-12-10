using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling the opacity of an element.
/// </summary>
internal class OpacityUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["opacity"];
    protected override string[] ThemeKeys => NamespaceResolver.OpacityChain;

    /// <summary>
    /// Handles bare numeric values by converting them to percentage values.
    /// Examples: "0" -> "0%", "50" -> "50%", "100" -> "100%".
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Try to parse as integer (0-100)
        if (int.TryParse(value, out var intValue) && intValue >= 0 && intValue <= 100)
        {
            // Keep as percentage for Tailwind v4 compatibility
            return $"{intValue}%";
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

    protected override string GetSampleCssForArbitraryValue(string pattern) => "opacity: [value]";
}