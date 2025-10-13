using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Utilities for controlling the maximum width of an element.
/// </summary>
internal class MaxWidthUtility : BaseSizingUtility
{
    protected override string[] Patterns => ["max-w"];

    protected override string[] SizingNamespaces => NamespaceResolver.AppendFallbacks(NamespaceResolver.MaxWidthChain, "--container");

    protected override SizingDimension Dimension => SizingDimension.Width;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "max-w":
                declarations.Add(new Declaration("max-width", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }

    protected override string GetSpecialSizingValue(string key)
    {
        // Include "none" as special value for max-width
        if (key == "none")
        {
            return "none";
        }

        // Don't handle "0" specially - let it go through numeric value handling
        return base.GetSpecialSizingValue(key);
    }
}