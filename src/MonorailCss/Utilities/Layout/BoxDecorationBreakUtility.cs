using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling how element fragments are rendered across multiple lines, columns, or pages.
/// </summary>
internal class BoxDecorationBreakUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "box-decoration-slice", ("box-decoration-break", "slice") },
            { "box-decoration-clone", ("box-decoration-break", "clone") },
        }.ToImmutableDictionary();

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        if (!StaticValues.ContainsKey(staticUtility.Root))
        {
            return false;
        }

        var (property, value) = StaticValues[staticUtility.Root];

        // Generate both webkit-prefixed and standard properties
        var declarations = ImmutableList.Create<AstNode>(
            new Declaration($"-webkit-{property}", value, candidate.Important),
            new Declaration(property, value, candidate.Important));

        results = declarations;
        return true;
    }
}