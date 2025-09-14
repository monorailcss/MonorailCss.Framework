using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Sizing;

/// <summary>
/// Handles max-width utilities (max-w-*).
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
        // Include "none" and "0" as special values for max-width
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