using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling the inset shadow of an element.
/// </summary>
internal class InsetShadowUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ExactStatic;

    public string[] GetNamespaces() => [];

    public string GetUtilityName() => "inset-shadow-none";

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility { Root: "inset-shadow-none" })
        {
            return false;
        }

        var declarations = ImmutableList.CreateBuilder<AstNode>();
        declarations.Add(new Declaration("--tw-inset-shadow", "0 0 #0000", candidate.Important));
        declarations.Add(new Declaration("box-shadow", "var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)", candidate.Important));

        results = declarations.ToImmutable();
        return true;
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register the inset-shadow custom property
        propertyRegistry.Register("--tw-inset-shadow", "*", false, "0 0 #0000");

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }

    /// <summary>
    /// Returns examples of inset shadow utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("inset-shadow-none", "Remove inset shadow"),
        };

        return examples;
    }
}