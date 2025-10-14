using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling the order of flex and grid items.
/// </summary>
internal class OrderUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["order"];
    protected override string[] ThemeKeys => NamespaceResolver.OrderChain;
    protected override bool SupportsNegative => true;

    /// <summary>
    /// Handles bare integer values for order.
    /// Examples: "0" -> "0", "1" -> "1", "12" -> "12".
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Allow integer values
        if (int.TryParse(value, out var intValue))
        {
            return intValue.ToString();
        }

        return null;
    }

    /// <summary>
    /// Validates arbitrary values for order (should be integers).
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        // Allow integer values
        if (int.TryParse(value, out _))
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
            new Declaration("order", value, important));
    }

    public string[]? GetDocumentedProperties() => ["order"];

    public override IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return new[]
        {
            new Documentation.UtilityExample("order-1", "Set order to 1"),
            new Documentation.UtilityExample("order-12", "Set order to 12"),
            new Documentation.UtilityExample("-order-1", "Set order to -1"),
            new Documentation.UtilityExample("order-[13]", "Use an arbitrary value for order"),
        };
    }
}