using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utilities for controlling the middle color of gradient color stops.
/// </summary>
internal class GradientViaUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.NamespaceHandler;

    public string[] GetNamespaces() => NamespaceResolver.GradientViaChain;

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != "via")
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

        // Set the gradient via color
        declarations.Add(new Declaration("--tw-gradient-via", color, candidate.Important));

        // Set the complex gradient via stops system
        var viaStops = "var(--tw-gradient-position), var(--tw-gradient-from) var(--tw-gradient-from-position), var(--tw-gradient-via) var(--tw-gradient-via-position), var(--tw-gradient-to) var(--tw-gradient-to-position)";
        declarations.Add(new Declaration("--tw-gradient-via-stops", viaStops, candidate.Important));

        // Use the via stops for the main gradient stops
        declarations.Add(new Declaration("--tw-gradient-stops", "var(--tw-gradient-via-stops)", candidate.Important));

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
    /// Returns examples of gradient via utilities with theme-aware color values.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("via-purple-500", "Set gradient middle color to purple-500"),
            new("via-pink-600", "Set gradient middle color to pink-600"),
            new("via-indigo-400", "Set gradient middle color to indigo-400"),
            new("via-yellow-500/50", "Set gradient middle color to yellow-500 with 50% opacity"),
            new("via-[#ff00ff]", "Set gradient middle color with arbitrary hex value"),
        };

        return examples;
    }

    /// <summary>
    /// This utility contributes to the background-image CSS property via gradients.
    /// </summary>
    public string[] GetDocumentedProperties() => ["background-image"];
}