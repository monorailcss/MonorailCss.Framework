using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Css;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Utilities for controlling the border width between elements.
/// </summary>
internal class DivideUtility : IUtility
{
    private static readonly string[] _functionalRoots = ["divide-x", "divide-y"];

    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public string[] GetNamespaces() => ["--divide-width", "--border-width"];

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

        // Handle directional width patterns (divide-x-*, divide-y-*)
        if (pattern is "divide-x" or "divide-y")
        {
            // Handle bare patterns (divide-x, divide-y) with default 1px width
            if (functionalUtility.Value == null)
            {
                results = CreateWidthDeclarations(pattern, "1px", candidate.Important);
                return true;
            }

            if (TryResolveAsWidth(functionalUtility.Value, theme, out var widthValue))
            {
                results = CreateWidthDeclarations(pattern, widthValue, candidate.Important);
                return true;
            }
        }

        return false;
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
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

        // Register CSS variables for divide functionality
        RegisterCssVariables(propertyRegistry);

        // Handle directional width patterns (divide-x-*, divide-y-*)
        if (pattern is "divide-x" or "divide-y")
        {
            // Handle bare patterns (divide-x, divide-y) with default 1px width
            if (functionalUtility.Value == null)
            {
                results = CreateWidthDeclarations(pattern, "1px", candidate.Important);
                return true;
            }

            if (TryResolveAsWidth(functionalUtility.Value, theme, out var widthValue))
            {
                results = CreateWidthDeclarations(pattern, widthValue, candidate.Important);
                return true;
            }
        }

        return false;
    }

    private static void RegisterCssVariables(CssPropertyRegistry propertyRegistry)
    {
        propertyRegistry.Register("--tw-divide-x-reverse", "*", false, "0");
        propertyRegistry.Register("--tw-divide-y-reverse", "*", false, "0");
        propertyRegistry.Register("--tw-border-style", "*", false, "solid");
    }

    private static bool TryResolveAsWidth(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? result)
    {
        // Try to resolve as border width using the dedicated resolver
        return ValueResolver.TryResolveBorderWidth(value, theme, out result);
    }

    private static ImmutableList<AstNode> CreateWidthDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();
        var childSelector = ":where(& > :not(:last-child))";

        if (pattern == "divide-x")
        {
            // Set the reverse variable with default value
            declarations.Add(new Declaration("--tw-divide-x-reverse", "0", important));

            // Set border style
            declarations.Add(new Declaration("border-inline-style", "var(--tw-border-style)", important));

            // Set border widths using calc with reverse variable
            declarations.Add(new Declaration(
                "border-inline-start-width",
                $"calc({value} * var(--tw-divide-x-reverse))",
                important));
            declarations.Add(new Declaration(
                "border-inline-end-width",
                $"calc({value} * calc(1 - var(--tw-divide-x-reverse)))",
                important));
        }
        else if (pattern == "divide-y")
        {
            // Set the reverse variable with default value
            declarations.Add(new Declaration("--tw-divide-y-reverse", "0", important));

            // Set border styles
            declarations.Add(new Declaration("border-top-style", "var(--tw-border-style)", important));
            declarations.Add(new Declaration("border-bottom-style", "var(--tw-border-style)", important));

            // Set border widths using calc with reverse variable
            declarations.Add(new Declaration(
                "border-top-width",
                $"calc({value} * var(--tw-divide-y-reverse))",
                important));
            declarations.Add(new Declaration(
                "border-bottom-width",
                $"calc({value} * calc(1 - var(--tw-divide-y-reverse)))",
                important));
        }

        return ImmutableList.Create<AstNode>(new NestedRule(childSelector, declarations.ToImmutableList()));
    }

    /// <summary>
    /// Returns examples of divide utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("divide-x", "Add 1px horizontal border between child elements"),
            new("divide-x-2", "Add 2px horizontal border between child elements"),
            new("divide-x-4", "Add 4px horizontal border between child elements"),
            new("divide-y", "Add 1px vertical border between child elements"),
            new("divide-y-2", "Add 2px vertical border between child elements"),
            new("divide-x-[3px]", "Add horizontal border with arbitrary width between child elements"),
        };

        return examples;
    }
}