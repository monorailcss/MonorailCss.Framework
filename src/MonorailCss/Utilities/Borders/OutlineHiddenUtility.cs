using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Utilities for hiding the outline of an element while maintaining accessibility.
/// </summary>
internal class OutlineHiddenUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ExactStatic;

    public string[] GetNamespaces() => [];

    public string GetUtilityName() => "outline-hidden";

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not StaticUtility { Root: "outline-hidden" })
        {
            return false;
        }

        var declarations = ImmutableList.CreateBuilder<AstNode>();

        // Set the custom property and direct property
        declarations.Add(new Declaration("--tw-outline-style", "none", candidate.Important));
        declarations.Add(new Declaration("outline-style", "none", candidate.Important));

        // Add forced-colors media query
        var mediaRuleDeclarations = ImmutableList.CreateBuilder<AstNode>();
        mediaRuleDeclarations.Add(new Declaration("outline", "2px solid transparent", candidate.Important));
        mediaRuleDeclarations.Add(new Declaration("outline-offset", "2px", candidate.Important));

        var mediaRule = new AtRule(
            "media",
            "(forced-colors: active)",
            mediaRuleDeclarations.ToImmutable());

        declarations.Add(mediaRule);

        results = declarations.ToImmutable();
        return true;
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register the outline style custom property
        propertyRegistry.Register("--tw-outline-style", "*", false, "solid");

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }

    /// <summary>
    /// Returns examples of outline-hidden utility.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("outline-hidden", "Hide outline but maintain accessibility with forced-colors support"),
        };

        return examples;
    }
}