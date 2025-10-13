using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utilities for controlling the starting color of gradient color stops.
/// </summary>
internal class GradientFromUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.NamespaceHandler;

    public string[] GetNamespaces() => NamespaceResolver.GradientFromChain;

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != "from")
        {
            return false;
        }

        if (functionalUtility.Value == null)
        {
            return false;
        }

        if (!TryResolveColor(functionalUtility.Value, theme, out var color))
        {
            return false;
        }

        // Opacity modifiers are now handled by ColorModifierStage in the pipeline
        var declarations = ImmutableList.CreateBuilder<AstNode>();

        // Set the gradient from color
        declarations.Add(new Declaration("--tw-gradient-from", color, candidate.Important));

        // Set the gradient stops system
        var gradientStops = "var(--tw-gradient-via-stops, var(--tw-gradient-position), var(--tw-gradient-from) var(--tw-gradient-from-position), var(--tw-gradient-to) var(--tw-gradient-to-position))";
        declarations.Add(new Declaration("--tw-gradient-stops", gradientStops, candidate.Important));

        results = declarations.ToImmutable();
        return true;
    }

    /// <summary>
    /// Resolves a color value using the gradient namespace chain.
    /// </summary>
    private bool TryResolveColor(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? color)
    {
        return ValueResolver.TryResolveColor(value, theme, GetNamespaces(), out color);
    }

    /// <summary>
    /// Returns examples of gradient from utilities with theme-aware color values.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("from-red-500", "Set gradient starting color to red-500"),
            new("from-blue-600", "Set gradient starting color to blue-600"),
            new("from-green-400", "Set gradient starting color to green-400"),
            new("from-purple-500/50", "Set gradient starting color to purple-500 with 50% opacity"),
            new("from-[#ff0000]", "Set gradient starting color with arbitrary hex value"),
        };

        return examples;
    }
}