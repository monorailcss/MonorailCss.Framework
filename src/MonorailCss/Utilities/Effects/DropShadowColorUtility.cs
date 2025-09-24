using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles drop shadow color utilities.
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
}