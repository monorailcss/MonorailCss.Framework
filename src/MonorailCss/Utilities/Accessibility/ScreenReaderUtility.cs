using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Accessibility;

/// <summary>
/// Utilities for controlling the visibility of content to screen readers.
/// </summary>
internal class ScreenReaderUtility : BaseStaticUtility
{
    // Define the static values (BaseStaticUtility requires this but we'll override TryCompile)
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            // These are dummy values since we override TryCompile to handle multiple properties
            { "sr-only", ("position", "absolute") },
            { "not-sr-only", ("position", "static") },
        }.ToImmutableDictionary();

    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        switch (staticUtility.Root)
        {
            case "sr-only":
                results = CreateSrOnlyDeclarations(candidate.Important);
                return true;

            case "not-sr-only":
                results = CreateNotSrOnlyDeclarations(candidate.Important);
                return true;

            default:
                return false;
        }
    }

    private static ImmutableList<AstNode> CreateSrOnlyDeclarations(bool important)
    {
        var declarations = new List<AstNode>
        {
            new Declaration("position", "absolute", important),
            new Declaration("width", "1px", important),
            new Declaration("height", "1px", important),
            new Declaration("padding", "0", important),
            new Declaration("margin", "-1px", important),
            new Declaration("overflow", "hidden", important),
            new Declaration("clip", "rect(0, 0, 0, 0)", important),
            new Declaration("white-space", "nowrap", important),
            new Declaration("border-width", "0", important),
        };

        return declarations.ToImmutableList();
    }

    private static ImmutableList<AstNode> CreateNotSrOnlyDeclarations(bool important)
    {
        var declarations = new List<AstNode>
        {
            new Declaration("position", "static", important),
            new Declaration("width", "auto", important),
            new Declaration("height", "auto", important),
            new Declaration("padding", "0", important),
            new Declaration("margin", "0", important),
            new Declaration("overflow", "visible", important),
            new Declaration("clip", "auto", important),
            new Declaration("white-space", "normal", important),
        };

        return declarations.ToImmutableList();
    }
}