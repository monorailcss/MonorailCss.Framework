using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utility for touch-action values with CSS variable support.
/// Handles: touch-auto, touch-none, touch-manipulation, touch-pan-x, touch-pan-y, etc.
/// CSS: touch-action property with CSS variables for pan and zoom combinations.
/// </summary>
internal class TouchActionUtility : BaseStaticUtility
{
    // Define all utility names so the parser can find them
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        new Dictionary<string, (string, string)>
        {
            // Simple static values
            ["touch-auto"] = ("touch-action", "auto"),
            ["touch-none"] = ("touch-action", "none"),
            ["touch-manipulation"] = ("touch-action", "manipulation"),

            // Pan values (dummy values - we override TryCompile)
            ["touch-pan-x"] = ("touch-action", "auto"),
            ["touch-pan-y"] = ("touch-action", "auto"),
            ["touch-pan-left"] = ("touch-action", "auto"),
            ["touch-pan-right"] = ("touch-action", "auto"),
            ["touch-pan-up"] = ("touch-action", "auto"),
            ["touch-pan-down"] = ("touch-action", "auto"),

            // Zoom values (dummy value - we override TryCompile)
            ["touch-pinch-zoom"] = ("touch-action", "auto"),
        }.ToImmutableDictionary();

    private static readonly Dictionary<string, (string Variable, string Value)> _panValues = new()
    {
        ["touch-pan-x"] = ("--tw-pan-x", "pan-x"),
        ["touch-pan-y"] = ("--tw-pan-y", "pan-y"),
        ["touch-pan-left"] = ("--tw-pan-x", "pan-left"),
        ["touch-pan-right"] = ("--tw-pan-x", "pan-right"),
        ["touch-pan-up"] = ("--tw-pan-y", "pan-up"),
        ["touch-pan-down"] = ("--tw-pan-y", "pan-down"),
    };

    private static readonly Dictionary<string, (string Variable, string Value)> _zoomValues = new()
    {
        ["touch-pinch-zoom"] = ("--tw-pinch-zoom", "pinch-zoom"),
    };

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register CSS variables for touch actions
        propertyRegistry.Register("--tw-pan-x", "*", false, string.Empty);
        propertyRegistry.Register("--tw-pan-y", "*", false, string.Empty);
        propertyRegistry.Register("--tw-pinch-zoom", "*", false, string.Empty);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        var name = staticUtility.Root;

        // Handle simple static values
        if (name is "touch-auto" or "touch-none" or "touch-manipulation")
        {
            // Use the base implementation for simple cases
            return base.TryCompile(candidate, theme, out results);
        }

        // Handle complex pan values that need multiple declarations
        if (_panValues.TryGetValue(name, out var panMapping))
        {
            var declarations = new List<AstNode>
            {
                new Declaration(panMapping.Variable, panMapping.Value, candidate.Important),
                new Declaration("touch-action", "var(--tw-pan-x,) var(--tw-pan-y,) var(--tw-pinch-zoom,)", candidate.Important),
            };
            results = declarations.ToImmutableList();
            return true;
        }

        // Handle zoom values
        if (_zoomValues.TryGetValue(name, out var zoomMapping))
        {
            var declarations = new List<AstNode>
            {
                new Declaration(zoomMapping.Variable, zoomMapping.Value, candidate.Important),
                new Declaration("touch-action", "var(--tw-pan-x,) var(--tw-pan-y,) var(--tw-pinch-zoom,)", candidate.Important),
            };
            results = declarations.ToImmutableList();
            return true;
        }

        return false;
    }
}