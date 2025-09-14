using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Handles divide color utilities for child element borders.
///
/// Color patterns: divide-{color}, divide-{color}/{opacity}
///
/// All utilities apply styles to child elements using :where(&amp; &gt; :not(:last-child)) selector.
/// Inherits from BaseColorUtility for automatic opacity modifier support.
/// </summary>
internal class DivideColorUtility : BaseColorUtility
{
    protected override string Pattern => "divide";

    protected override string CssProperty => "border-color";

    protected override string[] ColorNamespaces => NamespaceResolver.DivideColorChain;

    /// <summary>
    /// Override TryCompile to wrap the color declaration in child selector.
    /// </summary>
    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (!base.TryCompile(candidate, theme, out var baseResults) || baseResults == null)
        {
            return false;
        }

        // Wrap the declaration in the child selector
        var childSelector = ":where(& > :not(:last-child))";
        results = ImmutableList.Create<AstNode>(new NestedRule(childSelector, baseResults));
        return true;
    }
}