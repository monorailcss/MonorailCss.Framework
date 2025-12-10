using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling how flex items shrink.
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

    public string[] GetDocumentedProperties() => ["flex-shrink"];

    public override IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return
        [
            new Documentation.UtilityExample("shrink", "Allow flex item to shrink (defaults to 1)"),
            new Documentation.UtilityExample("shrink-0", "Prevent flex item from shrinking"),
            new Documentation.UtilityExample("flex-shrink", "Allow flex item to shrink"),
            new Documentation.UtilityExample("flex-shrink-0", "Prevent flex item from shrinking"),
            new Documentation.UtilityExample("shrink-[value]", "Set flex shrink factor with arbitrary value", "flex-shrink: [value]"),
        ];
    }
}