using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Handles height utilities (h-*).
/// </summary>
internal class HeightUtility : BaseSizingUtility
{
    protected override string[] Patterns => ["h"];

    protected override string[] SizingNamespaces => NamespaceResolver.HeightChain;

    protected override SizingDimension Dimension => SizingDimension.Height;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "h":
                declarations.Add(new Declaration("height", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }
}