using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.TransitionsAnimation;

/// <summary>
/// Utilities for controlling the easing of CSS transitions.
/// </summary>
internal class TransitionTimingFunctionUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["ease"];
    protected override string[] ThemeKeys => NamespaceResolver.EaseChain;

    /// <summary>
    /// Static timing function mappings for built-in easing functions.
    /// </summary>
    private static readonly ImmutableDictionary<string, TimingFunctionConfig> _timingFunctions =
        new Dictionary<string, TimingFunctionConfig>
        {
            ["linear"] = new TimingFunctionConfig("linear", "linear"),
            ["in"] = new TimingFunctionConfig("var(--ease-in)", "cubic-bezier(0.4, 0, 1, 1)"),
            ["out"] = new TimingFunctionConfig("var(--ease-out)", "cubic-bezier(0, 0, 0.2, 1)"),
            ["in-out"] = new TimingFunctionConfig("var(--ease-in-out)", "cubic-bezier(0.4, 0, 0.2, 1)"),
        }.ToImmutableDictionary();

    /// <summary>
    /// Handles bare values like "linear", "in", "out", "in-out" for predefined timing functions.
    /// Returns the CSS property value directly.
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Handle special "initial" value
        if (value == "initial")
        {
            return "initial";
        }

        // Handle predefined timing functions - return the CSS property value directly
        if (_timingFunctions.TryGetValue(value, out var config))
        {
            return config.PropertyValue;
        }

        // Let the base class handle theme resolution for other values
        return base.HandleBareValue(value);
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        // Check if this is a predefined timing function by looking up the original key
        foreach (var kvp in _timingFunctions)
        {
            if (kvp.Value.PropertyValue == value)
            {
                // Generate both CSS variable and property declarations for predefined functions
                return ImmutableList.Create<AstNode>(
                    new Declaration("--tw-ease", kvp.Value.CssVariableValue, important),
                    new Declaration("transition-timing-function", kvp.Value.PropertyValue, important));
            }
        }

        // Handle theme-resolved or arbitrary values - use the value for both properties
        return ImmutableList.Create<AstNode>(
            new Declaration("--tw-ease", value, important),
            new Declaration("transition-timing-function", value, important));
    }

    /// <summary>
    /// Validates arbitrary values for transition timing function.
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        // Allow CSS variables and calc expressions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow cubic-bezier functions
        if (value.StartsWith("cubic-bezier(") && value.EndsWith(")"))
        {
            return true;
        }

        // Allow standard timing function keywords
        var standardKeywords = new[] { "ease", "ease-in", "ease-out", "ease-in-out", "linear", "step-start", "step-end" };
        if (standardKeywords.Contains(value))
        {
            return true;
        }

        // Allow steps() functions
        if (value.StartsWith("steps(") && value.EndsWith(")"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Configuration for a timing function.
    /// </summary>
    private record TimingFunctionConfig(string CssVariableValue, string PropertyValue);

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register CSS variables for transition timing function
        propertyRegistry.Register("--tw-ease", "*", false, "ease");

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}