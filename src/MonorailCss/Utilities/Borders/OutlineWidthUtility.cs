using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Handles outline-width utilities.
///
/// Width patterns: outline-0, outline-1, outline-2, outline-4, outline-8
///
/// Maps to CSS outline-width property using spacing theme values.
/// Also sets the outline-style using CSS custom property.
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