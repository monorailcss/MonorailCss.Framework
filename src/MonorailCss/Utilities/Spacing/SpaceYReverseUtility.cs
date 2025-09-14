using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Spacing;

/// <summary>
/// Utility for reversing vertical spacing direction (space-y-reverse).
/// </summary>
internal class SpaceYReverseUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        ImmutableDictionary<string, (string Property, string Value)>.Empty
            .Add("space-y-reverse", ("--tw-space-y-reverse", "1"));

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register CSS variables for vertical spacing reverse
        propertyRegistry.Register("--tw-space-y-reverse", "*", false, "0");

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}