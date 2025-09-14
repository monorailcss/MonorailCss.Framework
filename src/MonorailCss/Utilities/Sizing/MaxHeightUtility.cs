using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Handles max-height utilities (max-h-*).
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
        // Include "none" and "0" as special values for max-height
        if (key == "none")
        {
            return "none";
        }

        if (key == "0")
        {
            return "0";
        }

        return base.GetSpecialSizingValue(key);
    }
}