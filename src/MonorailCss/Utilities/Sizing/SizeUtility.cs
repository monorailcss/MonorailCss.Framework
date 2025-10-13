using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Utilities for controlling the width and height of an element simultaneously.
/// </summary>
internal class SizeUtility : BaseSizingUtility
{
    protected override string[] Patterns => ["size"];

    protected override string[] SizingNamespaces => NamespaceResolver.BuildChain("--size", NamespaceResolver.Spacing);

    protected override SizingDimension Dimension => SizingDimension.Both;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "size":
                // size sets both width and height
                declarations.Add(new Declaration("width", value, important));
                declarations.Add(new Declaration("height", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }
}