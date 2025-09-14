using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utility for scroll-padding values.
/// Handles: scroll-p-0, scroll-p-px, scroll-p-0.5, scroll-p-1, scroll-p-2, scroll-p-4, scroll-p-8, scroll-p-[2rem], etc.
/// Also handles directional variants: scroll-px-*, scroll-py-*, scroll-pt-*, scroll-pr-*, scroll-pb-*, scroll-pl-*, scroll-ps-*, scroll-pe-*
/// CSS: scroll-padding: 0, scroll-padding: 1px, scroll-padding: var(--spacing-1), etc.
/// </summary>
internal class ScrollPaddingUtility : BaseSpacingUtility
{
    protected override string[] Patterns => [
        "scroll-p", "scroll-px", "scroll-py", "scroll-pt", "scroll-pr", "scroll-pb", "scroll-pl", "scroll-ps", "scroll-pe"
    ];

    protected override string[] SpacingNamespaces => NamespaceResolver.ScrollPaddingChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return pattern switch
        {
            "scroll-p" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-padding", value, important)),
            "scroll-px" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-padding-inline", value, important)),
            "scroll-py" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-padding-block", value, important)),
            "scroll-pt" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-padding-top", value, important)),
            "scroll-pr" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-padding-right", value, important)),
            "scroll-pb" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-padding-bottom", value, important)),
            "scroll-pl" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-padding-left", value, important)),
            "scroll-ps" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-padding-inline-start", value, important)),
            "scroll-pe" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-padding-inline-end", value, important)),
            _ => throw new InvalidOperationException($"Unsupported pattern: {pattern}"),
        };
    }
}