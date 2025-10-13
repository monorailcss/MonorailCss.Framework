using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Utilities for controlling the maximum height of an element.
/// </summary>
internal class MaxHeightUtility : BaseSizingUtility
{
    protected override string[] Patterns => ["max-h"];

    protected override string[] SizingNamespaces => NamespaceResolver.MaxHeightChain;

    protected override SizingDimension Dimension => SizingDimension.Height;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "max-h":
                declarations.Add(new Declaration("max-height", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }

    protected override string GetSpecialSizingValue(string key)
    {
        // Include "none" as special value for max-height
        if (key == "none")
        {
            return "none";
        }

        // Don't handle "0" specially - let it go through numeric value handling
        return base.GetSpecialSizingValue(key);
    }
}