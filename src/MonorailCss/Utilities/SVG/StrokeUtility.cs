using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.SVG;

/// <summary>
/// Handles SVG stroke utilities with dual functionality:
/// - Static values: stroke-none, stroke-current
/// - Stroke color: stroke-red-500, stroke-blue-600, etc.
/// - Stroke width: stroke-0, stroke-1, stroke-2, etc.
///
/// Similar to BorderUtility, determines value type and handles accordingly.
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
}