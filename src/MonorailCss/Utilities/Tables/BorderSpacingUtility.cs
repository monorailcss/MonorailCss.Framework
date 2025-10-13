using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Tables;

/// <summary>
/// Utilities for controlling the spacing between table borders.
/// </summary>
internal class BorderSpacingUtility : BaseSpacingUtility
{
    protected override string[] Patterns => ["border-spacing", "border-spacing-x", "border-spacing-y"];

    protected override string[] SpacingNamespaces => NamespaceResolver.BuildChain("--border-spacing", NamespaceResolver.Spacing);

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "border-spacing":
                // Sets both horizontal and vertical spacing
                declarations.Add(new Declaration("--tw-border-spacing-x", value, important));
                declarations.Add(new Declaration("--tw-border-spacing-y", value, important));
                declarations.Add(new Declaration("border-spacing", "var(--tw-border-spacing-x) var(--tw-border-spacing-y)", important));
                break;
            case "border-spacing-x":
                // Sets only horizontal spacing, preserving vertical
                declarations.Add(new Declaration("--tw-border-spacing-x", value, important));
                declarations.Add(new Declaration("border-spacing", "var(--tw-border-spacing-x) var(--tw-border-spacing-y)", important));
                break;
            case "border-spacing-y":
                // Sets only vertical spacing, preserving horizontal
                declarations.Add(new Declaration("--tw-border-spacing-y", value, important));
                declarations.Add(new Declaration("border-spacing", "var(--tw-border-spacing-x) var(--tw-border-spacing-y)", important));
                break;
        }

        return declarations.ToImmutableList();
    }

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important, CssPropertyRegistry propertyRegistry)
    {
        // Properties are now automatically registered by PropertyRegistrationStage in the pipeline
        return GenerateDeclarations(pattern, value, important);
    }
}