using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Utilities for controlling the maximum inline-size of an element.
/// </summary>
internal class MaxInlineSizeUtility : BaseSizingUtility
{
    protected override string[] Patterns => ["max-inline"];

    protected override string[] SizingNamespaces => NamespaceResolver.AppendFallbacks(NamespaceResolver.MaxWidthChain, "--container");

    protected override SizingDimension Dimension => SizingDimension.Width;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "max-inline":
                declarations.Add(new Declaration("max-inline-size", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }

    protected override string GetSpecialSizingValue(string key)
    {
        if (key == "none")
        {
            return "none";
        }

        return base.GetSpecialSizingValue(key);
    }
}
