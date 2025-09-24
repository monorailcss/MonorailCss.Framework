using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Handles border style utilities (border-solid, border-dashed, border-dotted, etc.).
/// Sets both --tw-border-style CSS variable and border-style property.
/// </summary>
internal class BorderStyleUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "border-solid", ("border-style", "solid") },
            { "border-dashed", ("border-style", "dashed") },
            { "border-dotted", ("border-style", "dotted") },
            { "border-double", ("border-style", "double") },
            { "border-hidden", ("border-style", "hidden") },
            { "border-none", ("border-style", "none") },
        }.ToImmutableDictionary();

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        if (!StaticValues.TryGetValue(staticUtility.Root, out var cssDeclaration))
        {
            return false;
        }

        // Generate both the CSS variable and the actual border-style property
        results = ImmutableList.Create<AstNode>(
            new Declaration("--tw-border-style", cssDeclaration.Value, candidate.Important),
            new Declaration("border-style", cssDeclaration.Value, candidate.Important));

        return true;
    }
}