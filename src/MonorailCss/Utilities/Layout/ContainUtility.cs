using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling the containment of an element's layout, style, and paint.
/// </summary>
internal class ContainUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            // These are placeholder values for discovery - actual implementation is in TryCompile override
            { "contain-none", ("contain", "none") },
            { "contain-strict", ("contain", "strict") },
            { "contain-content", ("contain", "content") },
            { "contain-size", ("contain", "size") },
            { "contain-inline-size", ("contain", "inline-size") },
            { "contain-layout", ("contain", "layout") },
            { "contain-paint", ("contain", "paint") },
            { "contain-style", ("contain", "style") },
        }.ToImmutableDictionary();

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        var containValue = "var(--tw-contain-size,) var(--tw-contain-layout,) var(--tw-contain-paint,) var(--tw-contain-style,)";

        switch (staticUtility.Root)
        {
            case "contain-none":
                results = ImmutableList.Create<AstNode>(
                    new Declaration("contain", "none", candidate.Important));
                return true;

            case "contain-strict":
                results = ImmutableList.Create<AstNode>(
                    new Declaration("contain", "strict", candidate.Important));
                return true;

            case "contain-content":
                results = ImmutableList.Create<AstNode>(
                    new Declaration("contain", "content", candidate.Important));
                return true;

            case "contain-size":
                results = ImmutableList.Create<AstNode>(
                    new Declaration("--tw-contain-size", "size", candidate.Important),
                    new Declaration("contain", containValue, candidate.Important));
                return true;

            case "contain-inline-size":
                results = ImmutableList.Create<AstNode>(
                    new Declaration("--tw-contain-size", "inline-size", candidate.Important),
                    new Declaration("contain", containValue, candidate.Important));
                return true;

            case "contain-layout":
                results = ImmutableList.Create<AstNode>(
                    new Declaration("--tw-contain-layout", "layout", candidate.Important),
                    new Declaration("contain", containValue, candidate.Important));
                return true;

            case "contain-paint":
                results = ImmutableList.Create<AstNode>(
                    new Declaration("--tw-contain-paint", "paint", candidate.Important),
                    new Declaration("contain", containValue, candidate.Important));
                return true;

            case "contain-style":
                results = ImmutableList.Create<AstNode>(
                    new Declaration("--tw-contain-style", "style", candidate.Important),
                    new Declaration("contain", containValue, candidate.Important));
                return true;

            default:
                return false;
        }
    }
}