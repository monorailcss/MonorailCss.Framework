using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utility for z-index values.
/// Handles: z-auto, z-0, z-10, z-50, -z-10, z-[100], etc.
/// CSS: z-index: auto, z-index: 0, z-index: 10, z-index: 50, z-index: -10, z-index: 100.
/// </summary>
internal class ZIndexUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["z"];
    protected override string[] ThemeKeys => NamespaceResolver.ZIndexChain;
    protected override bool SupportsNegative => true;

    /// <summary>
    /// Handles bare integer values for z-index.
    /// Examples: "0" -> "0", "10" -> "10", "50" -> "50", "auto" -> "auto".
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Handle static 'auto' value
        if (value == "auto")
        {
            return "auto";
        }

        // Allow integer values (including 0)
        if (int.TryParse(value, out var intValue))
        {
            return intValue.ToString();
        }

        return null;
    }

    /// <summary>
    /// Validates arbitrary values for z-index (should be integers).
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

        // Allow 'auto' for arbitrary values
        if (value == "auto")
        {
            return true;
        }

        return false;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("z-index", value, important));
    }
}