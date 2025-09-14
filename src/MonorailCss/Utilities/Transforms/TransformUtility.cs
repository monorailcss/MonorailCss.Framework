using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Handles transform enabler utilities (transform, transform-none).
/// The transform utility enables the CSS variable-based transform system,
/// while transform-none disables all transforms.
/// </summary>
internal class TransformUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "transform-none", ("transform", "none") },
        }.ToImmutableDictionary();

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        // Handle transform-none with the base implementation
        if (staticUtility.Root == "transform-none")
        {
            return base.TryCompile(candidate, theme, out results);
        }

        // Handle the main transform utility
        if (staticUtility.Root == "transform")
        {
            // Generate the transform declaration with CSS variables
            var declaration = new Declaration(
                "transform",
                "var(--tw-rotate-x,) var(--tw-rotate-y,) var(--tw-rotate-z,) var(--tw-skew-x,) var(--tw-skew-y,)",
                candidate.Important);
            results = ImmutableList.Create<AstNode>(declaration);
            return true;
        }

        return false;
    }

    public override IEnumerable<string> GetUtilityNames()
    {
        // Include transform in addition to the base static values
        return base.GetUtilityNames().ToImmutableHashSet().Add("transform");
    }
}