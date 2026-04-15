using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Utilities for controlling the maximum block-size of an element.
/// </summary>
internal class MaxBlockSizeUtility : BaseSizingUtility
{
    protected override string[] Patterns => ["max-block"];

    protected override string[] SizingNamespaces => NamespaceResolver.MaxHeightChain;

    protected override SizingDimension Dimension => SizingDimension.Height;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "max-block":
                declarations.Add(new Declaration("max-block-size", value, important));
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
