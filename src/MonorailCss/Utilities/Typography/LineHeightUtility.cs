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
        // Built-in keywords only. The named scale (tight/snug/normal/relaxed/loose) lives in the
        // --leading-* theme namespace and resolves to `var(--leading-<key>)` via base resolution,
        // matching Tailwind and respecting theme overrides.
        var namedValues = new Dictionary<string, string>
        {
            ["none"] = "1",
            ["px"] = "1px",
        };

        if (namedValues.TryGetValue(value, out var namedValue))
        {
            return namedValue;
        }

        // Tailwind v4 maps integer `leading-N` through the spacing scale:
        // line-height = calc(var(--spacing) * N). Returning a unitless multiplier here was
        // incorrect — the resulting `line-height: 6` reads as 6× font-size and cascades
        // through anything inheriting line-height (e.g. nested links inside a `lg:leading-6`
        // wrapper end up at 84px line-height for 14px text).
        if (int.TryParse(value, out var numValue) && numValue >= 0)
        {
            return $"calc(var(--spacing) * {numValue.ToString(CultureInfo.InvariantCulture)})";
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

    protected override string GetSampleCssForArbitraryValue(string pattern) => "line-height: [value]";
}