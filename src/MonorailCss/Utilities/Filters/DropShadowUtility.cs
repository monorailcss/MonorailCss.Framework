using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;

namespace MonorailCss.Utilities.Filters;

/// <summary>
/// Handles drop shadow utilities including drop-shadow-none.
/// </summary>
internal class DropShadowUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ExactStatic;

    public string[] GetNamespaces() => [];

    public string GetUtilityName() => "drop-shadow-none";

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility { Root: "drop-shadow-none" })
        {
            return false;
        }

        var declarations = ImmutableList.CreateBuilder<AstNode>();
        declarations.Add(new Declaration("--tw-drop-shadow", " ", candidate.Important));
        declarations.Add(new Declaration("filter", "var(--tw-blur,) var(--tw-brightness,) var(--tw-contrast,) var(--tw-grayscale,) var(--tw-hue-rotate,) var(--tw-invert,) var(--tw-saturate,) var(--tw-sepia,) var(--tw-drop-shadow,)", candidate.Important));

        results = declarations.ToImmutable();
        return true;
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register the drop-shadow custom property
        propertyRegistry.Register("--tw-drop-shadow", "*", false, null);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}