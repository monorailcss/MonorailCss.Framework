using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Spacing;

/// <summary>
/// Handles padding utilities (p-*, px-*, py-*, pt-*, pr-*, pb-*, pl-*, ps-*, pe-*).
/// </summary>
internal class PaddingUtility : BaseSpacingUtility
{
    protected override string[] Patterns => ["p", "px", "py", "pt", "pr", "pb", "pl", "ps", "pe"];

    protected override string[] SpacingNamespaces => NamespaceResolver.PaddingChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "p":
                declarations.Add(new Declaration("padding", value, important));
                break;
            case "px":
                declarations.Add(new Declaration("padding-inline", value, important));
                break;
            case "py":
                declarations.Add(new Declaration("padding-block", value, important));
                break;
            case "pt":
                declarations.Add(new Declaration("padding-top", value, important));
                break;
            case "pr":
                declarations.Add(new Declaration("padding-right", value, important));
                break;
            case "pb":
                declarations.Add(new Declaration("padding-bottom", value, important));
                break;
            case "pl":
                declarations.Add(new Declaration("padding-left", value, important));
                break;
            case "ps":
                declarations.Add(new Declaration("padding-inline-start", value, important));
                break;
            case "pe":
                declarations.Add(new Declaration("padding-inline-end", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }
}