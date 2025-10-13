using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Utilities for controlling the width of an element's outline.
/// </summary>
internal class OutlineWidthUtility : BaseSpacingUtility
{
    protected override string[] Patterns => ["outline"];

    protected override string[] SpacingNamespaces => NamespaceResolver.OutlineWidthChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("outline-style", "var(--tw-outline-style)", important),
            new Declaration("outline-width", value, important));
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important, CssPropertyRegistry propertyRegistry)
    {
        // Register the outline style custom property with default value
        propertyRegistry.Register("--tw-outline-style", "*", false, "solid");

        return ImmutableList.Create<AstNode>(
            new Declaration("outline-style", "var(--tw-outline-style)", important),
            new Declaration("outline-width", value, important));
    }
}