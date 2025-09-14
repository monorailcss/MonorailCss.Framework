using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utility for flex-shrink values.
/// Handles: shrink, flex-shrink (default to 1), shrink-0, flex-shrink-0, shrink-*, flex-shrink-*
/// CSS: flex-shrink: 1, flex-shrink: 0, flex-shrink: custom-value.
/// </summary>
internal class FlexShrinkUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["shrink", "flex-shrink"];
    protected override string[] ThemeKeys => NamespaceResolver.FlexShrinkChain;
    protected override string DefaultValue => "1"; // shrink without value defaults to 1

    /// <summary>
    /// Handles bare numeric values for flex-shrink.
    /// Examples: "0" -> "0", "1" -> "1".
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Allow numeric values (integers and decimals)
        if (double.TryParse(value, NumberStyles.Number,
            CultureInfo.InvariantCulture, out var numValue) && numValue >= 0)
        {
            return numValue.ToString("G", CultureInfo.InvariantCulture);
        }

        return null;
    }

    /// <summary>
    /// Validates arbitrary values for flex-shrink (should be non-negative numbers).
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        // Allow non-negative numeric values
        if (double.TryParse(value, out var numValue) && numValue >= 0)
        {
            return true;
        }

        // Allow CSS variables and calc expressions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        return false;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("flex-shrink", value, important));
    }
}