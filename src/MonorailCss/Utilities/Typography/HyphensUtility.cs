using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling how words should be hyphenated.
/// </summary>
internal class HyphensUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "hyphens-none", ("hyphens", "none") },
            { "hyphens-manual", ("hyphens", "manual") },
            { "hyphens-auto", ("hyphens", "auto") },
        }.ToImmutableDictionary();

    /// <summary>
    /// Generate CSS with vendor prefixes for hyphens properties.
    /// </summary>
    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        if (StaticValues.TryGetValue(staticUtility.Root, out var cssDeclaration))
        {
            // Generate both -webkit-hyphens and hyphens properties for compatibility
            results = ImmutableList.Create<AstNode>(
                new Declaration("-webkit-hyphens", cssDeclaration.Value, candidate.Important),
                new Declaration("hyphens", cssDeclaration.Value, candidate.Important));
            return true;
        }

        return false;
    }
}