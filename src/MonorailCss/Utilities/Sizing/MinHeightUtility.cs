using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Utilities for controlling the minimum height of an element.
/// </summary>
internal class MinHeightUtility : BaseSizingUtility
{
    protected override string[] Patterns => ["min-h"];

    protected override string[] SizingNamespaces => NamespaceResolver.MinHeightChain;

    protected override SizingDimension Dimension => SizingDimension.Height;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "min-h":
                declarations.Add(new Declaration("min-height", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }
}