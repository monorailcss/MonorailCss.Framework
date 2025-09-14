using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Spacing;

/// <summary>
/// Utility for vertical spacing between child elements (space-y-*).
/// </summary>
internal class SpaceYUtility : BaseSpacingUtility
{
    protected override string[] Patterns => ["space-y"];

    protected override string[] SpacingNamespaces => NamespaceResolver.SpaceChain;

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register CSS variables for vertical spacing
        propertyRegistry.Register("--tw-space-y-reverse", "*", false, "0");

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        // Create the child selector rule with all declarations inside
        var childSelector = ":where(& > :not(:last-child))";
        var childDeclarations = ImmutableList.CreateBuilder<AstNode>();

        // Set CSS variable for reverse direction with default value
        childDeclarations.Add(new Declaration("--tw-space-y-reverse", "0", important));

        // margin-block-start: calc(value * var(--tw-space-y-reverse))
        childDeclarations.Add(new Declaration(
            "margin-block-start",
            $"calc({value} * var(--tw-space-y-reverse))",
            important));

        // margin-block-end: calc(value * calc(1 - var(--tw-space-y-reverse)))
        childDeclarations.Add(new Declaration(
            "margin-block-end",
            $"calc({value} * calc(1 - var(--tw-space-y-reverse)))",
            important));

        // Return the nested rule as the only item
        return ImmutableList.Create<AstNode>(new NestedRule(childSelector, childDeclarations.ToImmutable()));
    }
}