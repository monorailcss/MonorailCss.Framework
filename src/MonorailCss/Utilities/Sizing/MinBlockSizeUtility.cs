using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Utilities for controlling the minimum block-size of an element.
/// </summary>
internal class MinBlockSizeUtility : BaseSizingUtility
{
    protected override string[] Patterns => ["min-block"];

    protected override string[] SizingNamespaces => NamespaceResolver.MinHeightChain;

    protected override SizingDimension Dimension => SizingDimension.Height;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "min-block":
                declarations.Add(new Declaration("min-block-size", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }
}
