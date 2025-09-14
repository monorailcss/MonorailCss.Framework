using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.TransitionsAnimation;

/// <summary>
/// Utility for transition property values.
/// Handles: transition, transition-none, transition-all, transition-colors, transition-opacity, transition-shadow, transition-transform
/// CSS: transition-property: [various], plus timing function and duration.
/// </summary>
internal class TransitionUtility : BaseFunctionalUtility
{
    protected override string[] Patterns => ["transition"];
    protected override string[] ThemeKeys => NamespaceResolver.TransitionChain;

    /// <summary>
    /// Predefined transition property groups for common use cases.
    /// </summary>
    private static readonly ImmutableDictionary<string, TransitionConfig> _transitionConfigs =
        new Dictionary<string, TransitionConfig>
        {
            // Default transition - covers most commonly animated properties
            [string.Empty] = new TransitionConfig(
                "color, background-color, border-color, outline-color, text-decoration-color, fill, stroke, opacity, box-shadow, transform, filter, backdrop-filter",
                true),

            // None - disables transitions
            ["none"] = new TransitionConfig("none", false),

            // All - animates all properties (use sparingly for performance)
            ["all"] = new TransitionConfig("all", true),

            // Colors - color-related properties only
            ["colors"] = new TransitionConfig(
                "color, background-color, border-color, outline-color, text-decoration-color, fill, stroke",
                true),

            // Opacity - opacity only
            ["opacity"] = new TransitionConfig("opacity", true),

            // Shadow - box-shadow only
            ["shadow"] = new TransitionConfig("box-shadow", true),

            // Transform - transform-related properties
            ["transform"] = new TransitionConfig("transform", true),
        }.ToImmutableDictionary();

    /// <summary>
    /// Handles bare values like "none", "all", "colors", etc. for predefined transitions.
    /// Returns the transition property value directly.
    /// </summary>
    protected override string? HandleBareValue(string value)
    {
        // Handle predefined transition configurations (including empty string for default transition)
        if (_transitionConfigs.TryGetValue(value, out var config))
        {
            return config.Properties;
        }

        // Let the base class handle theme resolution for other values
        return base.HandleBareValue(value);
    }

    /// <summary>
    /// Gets the default value for the transition utility when no value is specified (default transition).
    /// </summary>
    protected override string DefaultValue => _transitionConfigs[string.Empty].Properties;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        // Check if this is a predefined transition by looking up the properties value
        foreach (var kvp in _transitionConfigs)
        {
            if (kvp.Value.Properties == value)
            {
                var declarations = new List<AstNode>
                {
                    new Declaration("transition-property", kvp.Value.Properties, important),
                };

                // Add timing function and duration for non-none transitions
                if (kvp.Value.IncludeTimingAndDuration)
                {
                    declarations.Add(new Declaration("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)", important));
                    declarations.Add(new Declaration("transition-duration", "150ms", important));
                }

                return declarations.ToImmutableList();
            }
        }

        // Handle theme-resolved or arbitrary values (custom transition-property values)
        // Add timing and duration for these as well to match Tailwind behavior
        return ImmutableList.Create<AstNode>(
            new Declaration("transition-property", value, important),
            new Declaration("transition-timing-function", "cubic-bezier(0.4, 0, 0.2, 1)", important),
            new Declaration("transition-duration", "150ms", important));
    }

    /// <summary>
    /// Validates arbitrary values for transition-property.
    /// </summary>
    protected override bool IsValidArbitraryValue(string value)
    {
        // Allow CSS variables and calc expressions
        if (value.StartsWith("var(") || value.Contains("calc("))
        {
            return true;
        }

        // Allow valid CSS property names (basic validation)
        return !string.IsNullOrWhiteSpace(value) &&
               !value.Contains(';') &&
               !value.Contains('{') &&
               !value.Contains('}');
    }

    /// <summary>
    /// Configuration for a transition type.
    /// </summary>
    private record TransitionConfig(string Properties, bool IncludeTimingAndDuration);
}