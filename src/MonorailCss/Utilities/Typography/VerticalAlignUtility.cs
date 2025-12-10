using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the vertical alignment of an inline or table-cell box.
/// </summary>
internal class VerticalAlignUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["align"];

    protected override string[] ThemeKeys => NamespaceResolver.VerticalAlignChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("vertical-align", value, important));
    }

    protected override bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle static values first
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Handle vertical-align keywords
            resolvedValue = key switch
            {
                "baseline" => "baseline",
                "top" => "top",
                "middle" => "middle",
                "bottom" => "bottom",
                "text-top" => "text-top",
                "text-bottom" => "text-bottom",
                "sub" => "sub",
                "super" => "super",
                _ => string.Empty,
            };

            if (!string.Empty.Equals(resolvedValue))
            {
                return true;
            }
        }

        // Handle arbitrary values (align-[2px], align-[0.5em])
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Allow arbitrary length/percentage values
            if (IsValidVerticalAlignValue(arbitrary))
            {
                resolvedValue = arbitrary;
                return true;
            }
        }

        // Try theme resolution as fallback
        return base.TryResolveValue(value, theme, isNegative, out resolvedValue);
    }

    /// <summary>
    /// Validates if the arbitrary value is a valid vertical-align value.
    /// </summary>
    private static bool IsValidVerticalAlignValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Allow CSS keywords
        var keywords = new[] { "baseline", "top", "middle", "bottom", "text-top", "text-bottom", "sub", "super", "inherit", "initial", "unset", "revert" };
        if (keywords.Contains(value.Trim()))
        {
            return true;
        }

        // Allow CSS variables and functions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow length and percentage values
        var validUnits = new[] { "px", "em", "rem", "ch", "ex", "%", "vw", "vh", "vmin", "vmax" };
        var hasValidUnit = validUnits.Any(unit => value.EndsWith(unit));
        var isZero = value == "0";

        // Allow positive or negative numbers with units, or zero
        if (hasValidUnit || isZero)
        {
            return true;
        }

        return false;
    }

    protected override bool IsValidArbitraryValue(string value)
    {
        return IsValidVerticalAlignValue(value);
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "vertical-align: [value]";
}