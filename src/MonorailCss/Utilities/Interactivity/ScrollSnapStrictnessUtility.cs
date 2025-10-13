using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling how strictly snap points are enforced in a snap container.
/// </summary>
internal class ScrollSnapStrictnessUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        new Dictionary<string, (string, string)>
        {
            ["snap-mandatory"] = ("--tw-scroll-snap-strictness", "mandatory"),
            ["snap-proximity"] = ("--tw-scroll-snap-strictness", "proximity"),
        }.ToImmutableDictionary();

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register CSS variables for scroll snap strictness
        propertyRegistry.Register("--tw-scroll-snap-strictness", "*", false, "proximity");

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}