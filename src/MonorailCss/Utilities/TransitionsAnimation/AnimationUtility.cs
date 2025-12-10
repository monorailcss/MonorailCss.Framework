using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.TransitionsAnimation;

/// <summary>
/// Utilities for controlling CSS animations.
/// </summary>
internal class AnimationUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["animate"];
    protected override string[] ThemeKeys => NamespaceResolver.AnimateChain;

    /// <summary>
    /// Static animation mappings for built-in animations.
    /// </summary>
    private static readonly ImmutableDictionary<string, string> _staticAnimations =
        new Dictionary<string, string>
        {
            ["none"] = "none",
        }.ToImmutableDictionary();

    /// <summary>
    /// Built-in animation names that use theme variables.
    /// </summary>
    private static readonly ImmutableHashSet<string> _themeAnimations =
        new[] { "spin", "ping", "pulse", "bounce" }.ToImmutableHashSet();

    /// <summary>
    /// Handles bare values like "none" for static animations or theme-based values.
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Handle static values first
        if (_staticAnimations.TryGetValue(value, out var staticValue))
        {
            return staticValue;
        }

        // Handle theme animations - return CSS variable reference
        if (_themeAnimations.Contains(value))
        {
            return $"var(--animate-{value})";
        }

        // Let the base class handle theme resolution for other values
        return base.HandleBareValue(value);
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("animation", value, important));
    }

    /// <summary>
    /// Validates arbitrary values for animation (should be valid CSS animation shorthand).
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        // Allow CSS variables and calc expressions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow animation names with duration, timing, etc.
        // Basic validation - should not be empty and contain valid CSS identifiers
        return !string.IsNullOrWhiteSpace(value) &&
               !value.Contains(';') &&
               !value.Contains('{') &&
               !value.Contains('}');
    }

    protected override string GetSampleCssForArbitraryValue(string pattern) => "animation: [value]";
}