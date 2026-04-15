using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Utilities for controlling the inline-size of an element.
/// </summary>
internal class InlineSizeUtility : BaseSizingUtility
{
    protected override string[] Patterns => ["inline"];

    protected override string[] SizingNamespaces => NamespaceResolver.AppendFallbacks(NamespaceResolver.WidthChain, "--container");

    protected override SizingDimension Dimension => SizingDimension.Width;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "inline":
                declarations.Add(new Declaration("inline-size", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }
}
