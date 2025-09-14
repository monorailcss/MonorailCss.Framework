using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Handles ring-offset-width utilities.
///
/// Width patterns: ring-offset-0, ring-offset-1, ring-offset-2, ring-offset-4, ring-offset-8
/// Arbitrary: ring-offset-[10px]
///
/// Sets the --tw-ring-offset-width CSS variable.
/// </summary>
internal class RingOffsetWidthUtility : BaseSpacingUtility
{
    protected override string[] Patterns => ["ring-offset"];

    protected override string[] SpacingNamespaces => NamespaceResolver.RingOffsetWidthChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return ImmutableList.Create<AstNode>(
            new Declaration("--tw-ring-offset-width", value, important));
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important, CssPropertyRegistry propertyRegistry)
    {
        // Register default value for ring offset width
        propertyRegistry.Register("--tw-ring-offset-width", "<length>", false, "0px");

        return GenerateDeclarations(pattern, value, important);
    }
}