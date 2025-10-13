using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.TransitionsAnimation;

/// <summary>
/// Utilities for controlling the duration of CSS transitions.
/// </summary>
internal class TransitionDurationUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["duration"];
    protected override string[] ThemeKeys => NamespaceResolver.DurationChain;

    /// <summary>
    /// Handles bare numeric values by converting them to milliseconds.
    /// Examples: "75" -> "75ms", "300" -> "300ms", "1000" -> "1000ms".
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Handle special "initial" value
        if (value == "initial")
        {
            return "initial";
        }

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
            new Declaration("--tw-duration", value, important),
            new Declaration("transition-duration", value, important));
    }

    /// <summary>
    /// Validates arbitrary values for transition duration.
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

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register CSS variables for transition duration
        propertyRegistry.Register("--tw-duration", "*", false, "150ms");

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}