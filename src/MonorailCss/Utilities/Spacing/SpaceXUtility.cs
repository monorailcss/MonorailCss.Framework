using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Spacing;

/// <summary>
/// Utilities for controlling the horizontal space between child elements.
/// </summary>
internal class SpaceXUtility : BaseSpacingUtility
{
    protected override string[] Patterns => ["space-x"];

    protected override string[] SpacingNamespaces => NamespaceResolver.SpaceChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        // Create the child selector rule with all declarations inside
        var childSelector = ":where(& > :not(:last-child))";
        var childDeclarations = ImmutableList.CreateBuilder<AstNode>();

        // Set CSS variable for reverse direction with default value
        childDeclarations.Add(new Declaration("--tw-space-x-reverse", "0", important));

        // margin-inline-start: calc(value * var(--tw-space-x-reverse))
        childDeclarations.Add(new Declaration(
            "margin-inline-start",
            $"calc({value} * var(--tw-space-x-reverse))",
            important));

        // margin-inline-end: calc(value * calc(1 - var(--tw-space-x-reverse)))
        childDeclarations.Add(new Declaration(
            "margin-inline-end",
            $"calc({value} * calc(1 - var(--tw-space-x-reverse)))",
            important));

        // Return the nested rule as the only item
        return ImmutableList.Create<AstNode>(new NestedRule(childSelector, childDeclarations.ToImmutable()));
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important, CssPropertyRegistry propertyRegistry)
    {
        // Register the custom property
        propertyRegistry.Register("--tw-space-x-reverse", "*", false, "0");

        return GenerateDeclarations(pattern, value, important);
    }
}