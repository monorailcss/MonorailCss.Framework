using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Handles gradient from utilities (from-*).
/// Sets the starting color for gradients using CSS variables.
/// CSS: --tw-gradient-from: color; --tw-gradient-stops: ...
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
}