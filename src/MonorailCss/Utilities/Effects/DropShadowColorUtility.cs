using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling the color of drop shadow filters.
/// </summary>
internal class DropShadowColorUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ExactStatic;

    public string[] GetNamespaces() => [];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility staticUtility)
        {
            return false;
        }

        var declarations = ImmutableList.CreateBuilder<AstNode>();

        switch (staticUtility.Root)
        {
            case "drop-shadow-current":
                declarations.Add(new Declaration("--tw-drop-shadow-color", "color-mix(in oklab, currentColor var(--tw-drop-shadow-alpha), transparent)", candidate.Important));
                declarations.Add(new Declaration("--tw-drop-shadow", "var(--tw-drop-shadow-size)", candidate.Important));
                results = declarations.ToImmutable();
                return true;
            case "drop-shadow-inherit":
                declarations.Add(new Declaration("--tw-drop-shadow-color", "color-mix(in oklab, inherit var(--tw-drop-shadow-alpha), transparent)", candidate.Important));
                declarations.Add(new Declaration("--tw-drop-shadow", "var(--tw-drop-shadow-size)", candidate.Important));
                results = declarations.ToImmutable();
                return true;
            case "drop-shadow-transparent":
                declarations.Add(new Declaration("--tw-drop-shadow-color", "color-mix(in oklab, transparent var(--tw-drop-shadow-alpha), transparent)", candidate.Important));
                declarations.Add(new Declaration("--tw-drop-shadow", "var(--tw-drop-shadow-size)", candidate.Important));
                results = declarations.ToImmutable();
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns examples of drop shadow color utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("drop-shadow-current", "Set drop shadow color to currentColor"),
            new("drop-shadow-inherit", "Set drop shadow color to inherit"),
            new("drop-shadow-transparent", "Set drop shadow color to transparent"),
        };

        return examples;
    }
}