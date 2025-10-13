using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Spacing;

/// <summary>
/// Utilities for reversing the horizontal space between child elements.
/// </summary>
internal class SpaceXReverseUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        ImmutableDictionary<string, (string Property, string Value)>.Empty
            .Add("space-x-reverse", ("--tw-space-x-reverse", "1"));

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register CSS variables for horizontal spacing reverse
        propertyRegistry.Register("--tw-space-x-reverse", "*", false, "0");

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}