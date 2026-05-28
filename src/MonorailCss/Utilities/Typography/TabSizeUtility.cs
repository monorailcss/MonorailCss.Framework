using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the width of a tab character.
/// </summary>
internal class TabSizeUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["tab"];
    protected override string[] ThemeKeys => [];

    /// <summary>
    /// Handles bare non-negative integers (e.g. `tab-2` → `tab-size: 2;`).
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        if (int.TryParse(value, out var n) && n >= 0)
        {
            return n.ToString();
        }

        return null;
    }

    /// <summary>
    /// Validates arbitrary values for tab-size: integers, lengths, var()/calc().
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (int.TryParse(value, out _))
        {
            return true;
        }

        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Length values (e.g. `2ch`, `4px`) are also valid per the CSS spec.
        return char.IsDigit(value[0]) || value[0] == '.';
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("tab-size", value, important));
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "tab-size: [value]";
}
