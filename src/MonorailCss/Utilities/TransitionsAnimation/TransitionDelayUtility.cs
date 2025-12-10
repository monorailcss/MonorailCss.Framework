using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.TransitionsAnimation;

/// <summary>
/// Utilities for controlling the delay of CSS transitions.
/// </summary>
internal class TransitionDelayUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["delay"];
    protected override string[] ThemeKeys => NamespaceResolver.DelayChain;

    /// <summary>
    /// Handles bare numeric values by converting them to milliseconds.
    /// Examples: "75" -> "75ms", "300" -> "300ms", "1000" -> "1000ms".
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Try to parse as integer (milliseconds)
        if (int.TryParse(value, out var milliseconds) && milliseconds >= 0)
        {
            return $"{milliseconds}ms";
        }

        // Try to parse as decimal for fractional seconds
        if (decimal.TryParse(value, out var seconds) && seconds >= 0)
        {
            // Convert to milliseconds if it's less than 10 (assume it's seconds)
            if (seconds < 10)
            {
                var ms = (int)(seconds * 1000);
                return $"{ms}ms";
            }

            // Otherwise assume it's already milliseconds
            return $"{seconds}ms";
        }

        return null;
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("transition-delay", value, important));
    }

    /// <summary>
    /// Validates arbitrary values for transition delay.
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        // Allow CSS variables and calc expressions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow time values (ms, s)
        if (value.EndsWith("ms") || value.EndsWith("s"))
        {
            var numericPart = value.TrimEnd('m', 's');
            return double.TryParse(numericPart, out var time) && time >= 0;
        }

        // Allow pure numeric values (will be converted to ms)
        if (double.TryParse(value, out var numeric) && numeric >= 0)
        {
            return true;
        }

        return false;
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "transition-delay: [value]";
}