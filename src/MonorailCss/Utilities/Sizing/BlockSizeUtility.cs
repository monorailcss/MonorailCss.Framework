using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Utilities for controlling the block-size of an element.
/// </summary>
internal class BlockSizeUtility : BaseSizingUtility
{
    protected override string[] Patterns => ["block"];

    protected override string[] SizingNamespaces => NamespaceResolver.HeightChain;

    protected override SizingDimension Dimension => SizingDimension.Height;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "block":
                declarations.Add(new Declaration("block-size", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }
}
