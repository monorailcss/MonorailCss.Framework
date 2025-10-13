using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Utilities for controlling the minimum width of an element.
/// </summary>
internal class MinWidthUtility : BaseSizingUtility
{
    protected override string[] Patterns => ["min-w"];

    protected override string[] SizingNamespaces => NamespaceResolver.AppendFallbacks(NamespaceResolver.MinWidthChain, "--container");

    protected override SizingDimension Dimension => SizingDimension.Width;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "min-w":
                declarations.Add(new Declaration("min-width", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }
}