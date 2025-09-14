using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles font smoothing utilities (antialiased, subpixel-antialiased).
/// These require vendor-prefixed properties for cross-browser support.
/// </summary>
internal class FontSmoothingUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "antialiased", ("font-smoothing", "antialiased") },
            { "subpixel-antialiased", ("font-smoothing", "auto") },
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

        var (_, value) = StaticValues[staticUtility.Root];

        // Generate vendor-prefixed properties for font smoothing
        var declarations = ImmutableList.Create<AstNode>(
            new Declaration("-webkit-font-smoothing", value, candidate.Important),
            new Declaration("-moz-osx-font-smoothing", value == "antialiased" ? "grayscale" : "auto", candidate.Important));

        results = declarations;
        return true;
    }
}