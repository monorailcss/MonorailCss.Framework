using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.DataTypes;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Utilities for controlling the width and color of an element's outline.
/// </summary>
internal class OutlineUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public string[] GetNamespaces() => NamespaceResolver.AppendFallbacks(NamespaceResolver.OutlineColorChain, "--outline-width");

    public string[] GetFunctionalRoots() => ["outline"];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != "outline")
        {
            return false;
        }

        // Handle bare "outline" (default 1px width)
        if (functionalUtility.Value == null)
        {
            results = CreateWidthDeclarations("1px", candidate.Important);
            return true;
        }

        // Use data type inference for arbitrary values
        if (functionalUtility.Value.Kind == ValueKind.Arbitrary)
        {
            var allowedTypes = new[] { DataType.Color, DataType.LineWidth, DataType.Length, DataType.Number, DataType.Percentage };
            if (ValueResolver.TryInferAndResolve(
                functionalUtility.Value,
                theme,
                allowedTypes,
                NamespaceResolver.OutlineColorChain,
                out var resolvedValue,
                out var inferredType))
            {
                switch (inferredType)
                {
                    case DataType.LineWidth:
                    case DataType.Length:
                    case DataType.Number:
                    case DataType.Percentage:
                        results = CreateWidthDeclarations(resolvedValue, candidate.Important);
                        return true;
                    case DataType.Color:
                        // Apply opacity modifier if present for colors
                        if (candidate.Modifier != null && inferredType == DataType.Color)
                        {
                            if (ValueResolver.TryResolveOpacity(candidate.Modifier, theme, out var opacity))
                            {
                                resolvedValue = $"color-mix(in oklab, {resolvedValue} {opacity}, transparent)";
                            }
                        }

                        results = CreateColorDeclarations(resolvedValue, candidate.Important);
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }

        // For named values, try width first
        if (TryResolveAsWidth(functionalUtility.Value, theme, out var widthValue))
        {
            results = CreateWidthDeclarations(widthValue, candidate.Important);
            return true;
        }

        // Then try as color
        if (TryResolveAsColor(functionalUtility.Value, theme, candidate.Modifier, out var colorValue))
        {
            results = CreateColorDeclarations(colorValue, candidate.Important);
            return true;
        }

        return false;
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register default values for outline properties
        propertyRegistry.Register("--tw-outline-style", "*", false, "solid");

        // Delegate to the main TryCompile method
        return TryCompile(candidate, theme, out results);
    }

    private static bool TryResolveAsWidth(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? result)
    {
        result = null;

        // Check for known outline width values
        var knownWidths = new Dictionary<string, string>
        {
            ["0"] = "0px",
            ["1"] = "1px",
            ["2"] = "2px",
            ["4"] = "4px",
            ["8"] = "8px",
        };

        if (knownWidths.TryGetValue(value.Value, out var width))
        {
            result = width;
            return true;
        }

        // Try to resolve from theme
        var resolved = theme.ResolveValue($"--outline-width-{value.Value}", []);

        if (resolved != null)
        {
            result = resolved;
            return true;
        }

        // Check if it's a bare number that could be a width in pixels
        if (int.TryParse(value.Value, out var pixels) && pixels >= 0 && pixels <= 20)
        {
            result = $"{pixels}px";
            return true;
        }

        return false;
    }

    private bool TryResolveAsColor(CandidateValue value, Theme.Theme theme, Modifier? modifier, [NotNullWhen(true)] out string? result)
    {
        result = null;

        // First try to resolve the base color
        if (!ValueResolver.TryResolveColor(value, theme, NamespaceResolver.OutlineColorChain, out var baseColor))
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

    private static ImmutableList<AstNode> CreateWidthDeclarations(string value, bool important)
    {
        // Outline width sets both the style and width properties
        return ImmutableList.Create<AstNode>(
            new Declaration("outline-style", "var(--tw-outline-style)", important),
            new Declaration("outline-width", value, important));
    }

    private static ImmutableList<AstNode> CreateColorDeclarations(string value, bool important)
    {
        // Outline color only sets the color property
        return ImmutableList.Create<AstNode>(
            new Declaration("outline-color", value, important));
    }

    /// <summary>
    /// Returns examples of outline utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("outline", "Apply 1px outline with default style"),
            new("outline-2", "Apply 2px outline"),
            new("outline-4", "Apply 4px outline"),
            new("outline-red-500", "Set outline color to red-500"),
            new("outline-blue-600", "Set outline color to blue-600"),
            new("outline-red-500/50", "Set outline color to red-500 with 50% opacity"),
            new("outline-[3px]", "Apply outline with arbitrary width"),
        };

        return examples;
    }
}