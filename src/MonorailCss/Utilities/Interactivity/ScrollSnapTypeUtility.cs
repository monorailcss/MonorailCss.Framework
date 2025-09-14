using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utility for scroll-snap-type values with CSS variable support.
/// Handles: snap-none, snap-x, snap-y, snap-both
/// CSS: scroll-snap-type property with CSS variable for strictness.
/// </summary>
internal class ScrollSnapTypeUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        new Dictionary<string, (string, string)>
        {
            ["snap-none"] = ("scroll-snap-type", "none"),
            ["snap-x"] = ("scroll-snap-type", "x var(--tw-scroll-snap-strictness)"),
            ["snap-y"] = ("scroll-snap-type", "y var(--tw-scroll-snap-strictness)"),
            ["snap-both"] = ("scroll-snap-type", "both var(--tw-scroll-snap-strictness)"),
        }.ToImmutableDictionary();

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register CSS variables for scroll snap type
        propertyRegistry.Register("--tw-scroll-snap-strictness", "*", false, "proximity");

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}