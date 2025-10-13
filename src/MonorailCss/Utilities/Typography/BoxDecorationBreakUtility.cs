using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling how element fragments are rendered across multiple lines, columns, or pages.
/// </summary>
internal class BoxDecorationBreakUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "decoration-slice", ("box-decoration-break", "slice") },
            { "decoration-clone", ("box-decoration-break", "clone") },
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

        // Generate both vendor-prefixed and standard properties
        results = ImmutableList.Create<AstNode>(
            new Declaration("-webkit-box-decoration-break", cssDeclaration.Value, candidate.Important),
            new Declaration("box-decoration-break", cssDeclaration.Value, candidate.Important));

        return true;
    }
}