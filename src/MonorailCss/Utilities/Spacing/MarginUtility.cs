using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Spacing;

/// <summary>
/// Handles margin utilities (m-*, mx-*, my-*, mt-*, mr-*, mb-*, ml-*, ms-*, me-*).
/// </summary>
internal class MarginUtility : BaseSpacingUtility
{
    protected override string[] Patterns => ["m", "mx", "my", "mt", "mr", "mb", "ml", "ms", "me"];

    protected override string[] SpacingNamespaces => NamespaceResolver.MarginChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "m":
                declarations.Add(new Declaration("margin", value, important));
                break;
            case "mx":
                declarations.Add(new Declaration("margin-inline", value, important));
                break;
            case "my":
                declarations.Add(new Declaration("margin-block", value, important));
                break;
            case "mt":
                declarations.Add(new Declaration("margin-top", value, important));
                break;
            case "mr":
                declarations.Add(new Declaration("margin-right", value, important));
                break;
            case "mb":
                declarations.Add(new Declaration("margin-bottom", value, important));
                break;
            case "ml":
                declarations.Add(new Declaration("margin-left", value, important));
                break;
            case "ms":
                declarations.Add(new Declaration("margin-inline-start", value, important));
                break;
            case "me":
                declarations.Add(new Declaration("margin-inline-end", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }
}