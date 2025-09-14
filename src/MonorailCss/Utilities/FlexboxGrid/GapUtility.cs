using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Handles gap utilities (gap-*, gap-x-*, gap-y-*).
/// </summary>
internal class GapUtility : BaseSpacingUtility
{
    protected override string[] Patterns => ["gap", "gap-x", "gap-y"];

    protected override string[] SpacingNamespaces => NamespaceResolver.GapChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "gap":
                declarations.Add(new Declaration("gap", value, important));
                break;
            case "gap-x":
                declarations.Add(new Declaration("column-gap", value, important));
                break;
            case "gap-y":
                declarations.Add(new Declaration("row-gap", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }
}