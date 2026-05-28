using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utilities for controlling the magnification (zoom) level of an element.
/// </summary>
internal class ZoomUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["zoom"];
    protected override string[] ThemeKeys => [];

    /// <summary>
    /// Bare integers become percentages (e.g. `zoom-150` → `zoom: 150%;`).
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        if (int.TryParse(value, out var n) && n >= 0)
        {
            return $"{n}%";
        }

        return null;
    }

    /// <summary>
    /// Arbitrary values pass through unchanged: integers, decimals, percentages,
    /// var()/calc().
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            return true;
        }

        if (value.EndsWith('%') &&
            double.TryParse(value.AsSpan(0, value.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            return true;
        }

        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        return value == "normal" || value == "reset";
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("zoom", value, important));
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "zoom: [value]";
}
