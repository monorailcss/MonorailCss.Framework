using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling the scroll offset around items in a snap container.
/// </summary>
internal class ScrollMarginUtility : BaseSpacingUtility
{
    protected override string[] Patterns => [
        "scroll-m", "scroll-mx", "scroll-my", "scroll-mt", "scroll-mr", "scroll-mb", "scroll-ml", "scroll-ms", "scroll-me"
    ];

    protected override string[] SpacingNamespaces => NamespaceResolver.ScrollMarginChain;

    protected override ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important)
    {
        return pattern switch
        {
            "scroll-m" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-margin", value, important)),
            "scroll-mx" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-margin-inline", value, important)),
            "scroll-my" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-margin-block", value, important)),
            "scroll-mt" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-margin-top", value, important)),
            "scroll-mr" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-margin-right", value, important)),
            "scroll-mb" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-margin-bottom", value, important)),
            "scroll-ml" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-margin-left", value, important)),
            "scroll-ms" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-margin-inline-start", value, important)),
            "scroll-me" => ImmutableList.Create<AstNode>(
                new Declaration("scroll-margin-inline-end", value, important)),
            _ => throw new InvalidOperationException($"Unsupported pattern: {pattern}"),
        };
    }
}