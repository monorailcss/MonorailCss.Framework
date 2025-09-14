using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Handles outline-offset utilities.
///
/// Offset patterns: outline-offset-0, outline-offset-1, outline-offset-2, etc.
/// Negative patterns: -outline-offset-1, -outline-offset-2, etc.
///
/// Maps to CSS outline-offset property using spacing theme values.
/// </summary>
internal class OutlineOffsetUtility : BaseSpacingUtility
{
    protected override string[] Patterns => ["outline-offset"];

    protected override string[] SpacingNamespaces => NamespaceResolver.OutlineOffsetChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("outline-offset", value, important));
    }
}