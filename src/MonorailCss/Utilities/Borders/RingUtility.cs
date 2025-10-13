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
/// Utilities for controlling the width and color of ring shadows.
/// </summary>
internal class RingUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public string[] GetNamespaces() => NamespaceResolver.AppendFallbacks(NamespaceResolver.RingColorChain, "--ring-width", "--border-width", "--spacing");

    public string[] GetFunctionalRoots() => ["ring"];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != "ring")
        {
            return false;
        }

        // Handle bare "ring" (default 3px width)
        if (functionalUtility.Value == null)
        {
            results = CreateWidthDeclarations("3px", candidate.Important);
            return true;
        }

        // Use data type inference for arbitrary values
        if (functionalUtility.Value.Kind == ValueKind.Arbitrary)
        {
            var allowedTypes = new[] { DataType.Color, DataType.LineWidth, DataType.Length };
            if (ValueResolver.TryInferAndResolve(
                functionalUtility.Value,
                theme,
                allowedTypes,
                NamespaceResolver.RingColorChain,
                out var resolvedValue,
                out var inferredType))
            {
                switch (inferredType)
                {
                    case DataType.LineWidth:
                    case DataType.Length:
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
        results = null;

        // Register default values for ring properties
        propertyRegistry.Register("--tw-ring-inset", "*", false, null);
        propertyRegistry.Register("--tw-ring-offset-width", "<length>", false, "0px");
        propertyRegistry.Register("--tw-ring-offset-color", "<color>", false, "#fff");
        propertyRegistry.Register("--tw-ring-color", "<color>", false, "rgb(59 130 246 / 0.5)"); // blue-500 with 50% opacity
        propertyRegistry.Register("--tw-shadow", "*", false, "0 0 transparent");

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != "ring")
        {
            return false;
        }

        // Handle bare "ring" (default 3px width)
        if (functionalUtility.Value == null)
        {
            results = CreateWidthDeclarations("3px", candidate.Important);
            return true;
        }

        // Use data type inference for arbitrary values
        if (functionalUtility.Value.Kind == ValueKind.Arbitrary)
        {
            var allowedTypes = new[] { DataType.Color, DataType.LineWidth, DataType.Length };
            if (ValueResolver.TryInferAndResolve(
                functionalUtility.Value,
                theme,
                allowedTypes,
                NamespaceResolver.RingColorChain,
                out var resolvedValue,
                out var inferredType))
            {
                switch (inferredType)
                {
                    case DataType.LineWidth:
                    case DataType.Length:
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

    private static bool TryResolveAsWidth(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? result)
    {
        result = null;

        // Check for known ring width values
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
        var resolved = theme.ResolveValue($"--ring-width-{value.Value}", [])
                    ?? theme.ResolveValue($"--border-width-{value.Value}", []);

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
        if (!ValueResolver.TryResolveColor(value, theme, NamespaceResolver.RingColorChain, out var baseColor))
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
        // Ring uses box-shadow: 0 0 0 <width> <color>
        // We use CSS variables for composability
        return ImmutableList.Create<AstNode>(
            new Declaration("--tw-ring-shadow", $"var(--tw-ring-inset,) 0 0 0 calc({value} + var(--tw-ring-offset-width)) var(--tw-ring-color, currentColor)", important),
            new Declaration("box-shadow", "var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)", important));
    }

    private static ImmutableList<AstNode> CreateColorDeclarations(string value, bool important)
    {
        // Ring color sets the --tw-ring-color variable
        return ImmutableList.Create<AstNode>(
            new Declaration("--tw-ring-color", value, important));
    }

    /// <summary>
    /// Returns examples of ring utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("ring", "Apply 3px ring with default color"),
            new("ring-2", "Apply 2px ring"),
            new("ring-4", "Apply 4px ring"),
            new("ring-red-500", "Set ring color to red-500"),
            new("ring-blue-600", "Set ring color to blue-600"),
            new("ring-red-500/50", "Set ring color to red-500 with 50% opacity"),
            new("ring-[3px]", "Apply ring with arbitrary width"),
            new("ring-[#ff0000]", "Set ring color with arbitrary hex value"),
        };

        return examples;
    }
}