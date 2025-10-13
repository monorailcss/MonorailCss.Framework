using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling an element's scroll offset within a snap container.
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