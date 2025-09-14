using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Handles border radius utilities for rounding corners.
///
/// Global patterns: rounded, rounded-none, rounded-sm, rounded-md, rounded-lg, etc.
/// Directional patterns: rounded-t-*, rounded-r-*, rounded-b-*, rounded-l-* (2 corners each)
/// Individual corner patterns: rounded-tl-*, rounded-tr-*, rounded-bl-*, rounded-br-*
/// Logical corner patterns: rounded-ss-*, rounded-se-*, rounded-ee-*, rounded-es-*
/// Arbitrary values: rounded-[10px], rounded-t-[5px], etc.
/// </summary>
internal class BorderRadiusUtility : IUtility
{
    private static readonly string[] _functionalRoots = [
        "rounded", "rounded-t", "rounded-r", "rounded-b", "rounded-l",
        "rounded-tl", "rounded-tr", "rounded-bl", "rounded-br",
        "rounded-ss", "rounded-se", "rounded-ee", "rounded-es"
    ];

    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public string[] GetNamespaces() => ["--border-radius", "--radius"];

    public string[] GetFunctionalRoots() => _functionalRoots;

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        var pattern = functionalUtility.Root;
        if (!_functionalRoots.Contains(pattern))
        {
            return false;
        }

        // Handle bare "rounded" pattern (use default 0.25rem)
        if (functionalUtility.Value == null && pattern == "rounded")
        {
            results = CreateRadiusDeclarations(pattern, "0.25rem", candidate.Important);
            return true;
        }

        // All other patterns require a value
        if (functionalUtility.Value == null)
        {
            return false;
        }

        // Resolve the radius value
        if (ValueResolver.TryResolveBorderRadius(functionalUtility.Value, theme, out var radiusValue))
        {
            results = CreateRadiusDeclarations(pattern, radiusValue, candidate.Important);
            return true;
        }

        return false;
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Border radius utilities don't need to register CSS variables
        return TryCompile(candidate, theme, out results);
    }

    private static ImmutableList<AstNode> CreateRadiusDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "rounded":
                declarations.Add(new Declaration("border-radius", value, important));
                break;

            // Directional patterns (2 corners each)
            case "rounded-t":
                declarations.Add(new Declaration("border-top-left-radius", value, important));
                declarations.Add(new Declaration("border-top-right-radius", value, important));
                break;
            case "rounded-r":
                declarations.Add(new Declaration("border-top-right-radius", value, important));
                declarations.Add(new Declaration("border-bottom-right-radius", value, important));
                break;
            case "rounded-b":
                declarations.Add(new Declaration("border-bottom-right-radius", value, important));
                declarations.Add(new Declaration("border-bottom-left-radius", value, important));
                break;
            case "rounded-l":
                declarations.Add(new Declaration("border-top-left-radius", value, important));
                declarations.Add(new Declaration("border-bottom-left-radius", value, important));
                break;

            // Individual corner patterns
            case "rounded-tl":
                declarations.Add(new Declaration("border-top-left-radius", value, important));
                break;
            case "rounded-tr":
                declarations.Add(new Declaration("border-top-right-radius", value, important));
                break;
            case "rounded-bl":
                declarations.Add(new Declaration("border-bottom-left-radius", value, important));
                break;
            case "rounded-br":
                declarations.Add(new Declaration("border-bottom-right-radius", value, important));
                break;

            // Logical corner patterns
            case "rounded-ss":
                declarations.Add(new Declaration("border-start-start-radius", value, important));
                break;
            case "rounded-se":
                declarations.Add(new Declaration("border-start-end-radius", value, important));
                break;
            case "rounded-ee":
                declarations.Add(new Declaration("border-end-end-radius", value, important));
                break;
            case "rounded-es":
                declarations.Add(new Declaration("border-end-start-radius", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }
}