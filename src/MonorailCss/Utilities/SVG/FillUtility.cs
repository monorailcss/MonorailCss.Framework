using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.SVG;

/// <summary>
/// Utilities for controlling the fill color of SVG elements.
/// </summary>
internal class FillUtility : IUtility
{
    private static readonly ImmutableDictionary<string, string> _staticValues =
        new Dictionary<string, string>
        {
            { "none", "none" },
            { "current", "currentcolor" },
        }.ToImmutableDictionary();

    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public string[] GetNamespaces() => NamespaceResolver.FillColorChain;

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        // Handle functional values (colors and static values)
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

        // Check for static values first
        if (_staticValues.TryGetValue(functionalUtility.Value.Value, out var staticValue))
        {
            results = ImmutableList.Create<AstNode>(new Declaration("fill", staticValue, candidate.Important));
            return true;
        }

        // Try to resolve as a color
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

    /// <summary>
    /// Returns examples of SVG fill utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("fill-none", "Remove SVG fill"),
            new("fill-current", "Set SVG fill to currentColor"),
            new("fill-red-500", "Set SVG fill color to red-500"),
            new("fill-blue-600", "Set SVG fill color to blue-600"),
            new("fill-red-500/50", "Set SVG fill color to red-500 with 50% opacity"),
            new("fill-[#ff0000]", "Set SVG fill with arbitrary hex color"),
        };

        return examples;
    }
}