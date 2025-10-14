using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utilities for controlling the ending color of gradient color stops.
/// </summary>
internal class GradientToUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.NamespaceHandler;

    public string[] GetNamespaces() => NamespaceResolver.GradientToChain;

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != "to")
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

        // Set the gradient to color
        declarations.Add(new Declaration("--tw-gradient-to", color, candidate.Important));

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
    /// Returns examples of gradient to utilities with theme-aware color values.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("to-blue-500", "Set gradient ending color to blue-500"),
            new("to-purple-600", "Set gradient ending color to purple-600"),
            new("to-pink-400", "Set gradient ending color to pink-400"),
            new("to-cyan-500/50", "Set gradient ending color to cyan-500 with 50% opacity"),
            new("to-[#0000ff]", "Set gradient ending color with arbitrary hex value"),
        };

        return examples;
    }

    /// <summary>
    /// This utility contributes to the background-image CSS property via gradients.
    /// </summary>
    public string[] GetDocumentedProperties() => ["background-image"];
}