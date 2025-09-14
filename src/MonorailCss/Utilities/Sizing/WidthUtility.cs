using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Handles width utilities (w-*).
/// </summary>
internal class WidthUtility : BaseSizingUtility
{
    protected override string[] Patterns => ["w"];

    protected override string[] SizingNamespaces => NamespaceResolver.AppendFallbacks(NamespaceResolver.WidthChain, "--container");

    protected override SizingDimension Dimension => SizingDimension.Width;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "w":
                declarations.Add(new Declaration("width", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }
}