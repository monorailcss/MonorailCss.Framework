using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the color of placeholder text (<c>placeholder-red-500</c>,
/// <c>placeholder-[#0088cc]</c>, <c>placeholder-(--c)</c>, and the
/// <c>current</c>/<c>inherit</c>/<c>transparent</c> keywords).
/// </summary>
/// <remarks>
/// Emits a nested <c>&amp;::placeholder { color: … }</c> rule (theme colors resolve to
/// <c>var(--color-*)</c>), mirroring <c>DivideColorUtility</c>'s child-selector wrapping. Unlike the
/// shadow colors there is no <c>color-mix</c>/alpha machinery — the color is set directly.
/// </remarks>
internal class PlaceholderColorUtility : BaseColorUtility
{
    protected override string Pattern => "placeholder";

    protected override string CssProperty => "color";

    protected override string[] ColorNamespaces => NamespaceResolver.PlaceholderColorChain;

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (!base.TryCompile(candidate, theme, out var baseResults) || baseResults == null)
        {
            return false;
        }

        results = ImmutableList.Create<AstNode>(new NestedRule("&::placeholder", baseResults));
        return true;
    }
}
