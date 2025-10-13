using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.SVG;

/// <summary>
/// Utilities for controlling the stroke color and width of SVG elements.
/// </summary>
internal class StrokeUtility : IUtility
{
    private static readonly ImmutableDictionary<string, string> _staticValues =
        new Dictionary<string, string>
        {
            { "none", "none" },
            { "current", "currentcolor" },
        }.ToImmutableDictionary();

    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public string[] GetNamespaces() => NamespaceResolver.AppendFallbacks(NamespaceResolver.StrokeColorChain, "--stroke-width");

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        // Handle functional values
        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != "stroke")
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
            results = ImmutableList.Create<AstNode>(new Declaration("stroke", staticValue, candidate.Important));
            return true;
        }

        // Try to resolve as stroke width first (numeric values)
        if (TryResolveAsStrokeWidth(functionalUtility.Value, theme, out var widthValue))
        {
            results = ImmutableList.Create<AstNode>(new Declaration("stroke-width", widthValue, candidate.Important));
            return true;
        }

        // Try to resolve as stroke color
        if (TryResolveAsColor(functionalUtility.Value, theme, candidate.Modifier, out var colorValue))
        {
            results = ImmutableList.Create<AstNode>(new Declaration("stroke", colorValue, candidate.Important));
            return true;
        }

        return false;
    }

    private static bool TryResolveAsStrokeWidth(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? result)
    {
        result = null;

        // Handle basic numeric values (0, 1, 2, etc.)
        if (value.Kind == ValueKind.Named && int.TryParse(value.Value, out var intValue))
        {
            result = intValue.ToString();
            return true;
        }

        // Try to resolve from theme stroke-width (fallback to border-width for now)
        return ValueResolver.TryResolveBorderWidth(value, theme, out result);
    }

    private bool TryResolveAsColor(CandidateValue value, Theme.Theme theme, Modifier? modifier, [NotNullWhen(true)] out string? result)
    {
        result = null;

        // First try to resolve the base color
        if (!ValueResolver.TryResolveColor(value, theme, NamespaceResolver.StrokeColorChain, out var baseColor))
        {
            return false;
        }

        // Apply opacity modifier if present
        if (modifier != null)
        {
            // Only apply opacity modifiers, not other types like "negative"
            if (modifier.Value != "negative" && ValueResolver.TryResolveOpacity(modifier, theme, out var opacity))
            {
                // Use color-mix() to apply opacity
                result = $"color-mix(in oklab, {baseColor} {opacity}, transparent)";
                return true;
            }
        }

        result = baseColor;
        return true;
    }

    /// <summary>
    /// Returns examples of SVG stroke utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("stroke-none", "Remove SVG stroke"),
            new("stroke-current", "Set SVG stroke to currentColor"),
            new("stroke-0", "Set SVG stroke width to 0"),
            new("stroke-1", "Set SVG stroke width to 1"),
            new("stroke-2", "Set SVG stroke width to 2"),
            new("stroke-red-500", "Set SVG stroke color to red-500"),
            new("stroke-blue-600", "Set SVG stroke color to blue-600"),
            new("stroke-red-500/50", "Set SVG stroke color to red-500 with 50% opacity"),
        };

        return examples;
    }
}