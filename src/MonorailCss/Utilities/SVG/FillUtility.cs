using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.SVG;

/// <summary>
/// Handles SVG fill color utilities (fill-red-500, fill-blue-600, etc.).
/// Supports color values with opacity modifiers.
/// Static values (fill-none, fill-current) are handled by FillStaticUtility.
/// </summary>
internal class FillUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public string[] GetNamespaces() => NamespaceResolver.FillColorChain;

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        // Handle functional values (colors)
        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != "fill")
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
        results = ImmutableList.Create<AstNode>(new Declaration("fill", color, candidate.Important));
        return true;
    }

    private bool TryResolveColor(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? color)
    {
        return ValueResolver.TryResolveColor(value, theme, NamespaceResolver.FillColorChain, out color);
    }
}