using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling gutters between grid and flexbox items.
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

    public string[] GetDocumentedProperties() => ["gap"];

    public override IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return
        [
            new Documentation.UtilityExample("gap-0", "Remove gap between items"),
            new Documentation.UtilityExample("gap-4", "Set gap between items to 1rem"),
            new Documentation.UtilityExample("gap-8", "Set gap between items to 2rem"),
            new Documentation.UtilityExample("gap-x-4", "Set horizontal gap between items"),
            new Documentation.UtilityExample("gap-y-4", "Set vertical gap between items"),
            new Documentation.UtilityExample("gap-[2rem]", "Use an arbitrary value for gap"),
        ];
    }
}