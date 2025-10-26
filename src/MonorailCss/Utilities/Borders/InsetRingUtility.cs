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
/// Utilities for controlling the width and color of inset ring shadows.
/// </summary>
internal class InsetRingUtility : IUtility
{
    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public string[] GetNamespaces() => NamespaceResolver.AppendFallbacks(NamespaceResolver.RingColorChain, "--ring-width", "--border-width", "--spacing");

    public string[] GetFunctionalRoots() => ["inset-ring"];

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != "inset-ring")
        {
            return false;
        }

        // Handle bare "inset-ring" (default 1px width)
        if (functionalUtility.Value == null)
        {
            results = CreateWidthDeclarations("1px", candidate.Important);
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

    private static bool TryResolveAsColor(CandidateValue value, Theme.Theme theme, Modifier? modifier, [NotNullWhen(true)] out string? result)
    {
        result = null;

        // Handle special color keywords
        if (value.Value == "current")
        {
            result = "currentColor";
            return true;
        }

        if (value.Value == "transparent")
        {
            result = "transparent";
            return true;
        }

        if (value.Value == "inherit")
        {
            result = "inherit";
            return true;
        }

        // Try to resolve as color
        if (ValueResolver.TryResolveColor(value, theme, NamespaceResolver.RingColorChain, out var colorValue))
        {
            // Apply opacity modifier if present
            if (modifier != null && ValueResolver.TryResolveOpacity(modifier, theme, out var opacity))
            {
                result = $"color-mix(in oklab, {colorValue} {opacity}, transparent)";
            }
            else
            {
                result = colorValue;
            }

            return true;
        }

        return false;
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != "inset-ring")
        {
            return false;
        }

        // Register CSS variables for inset ring
        propertyRegistry.Register("--tw-inset-ring-shadow", "*", false, "0 0 #0000");
        propertyRegistry.Register("--tw-inset-ring-color", "*", false, null);
        propertyRegistry.Register("--tw-inset-shadow", "*", false, "0 0 #0000");

        return TryCompile(candidate, theme, out results);
    }

    private static ImmutableList<AstNode> CreateWidthDeclarations(string value, bool important)
    {
        // Inset ring width sets an inset box-shadow with the ring
        return ImmutableList.Create<AstNode>(
            new Declaration("--tw-inset-ring-shadow", $"inset 0 0 0 {value} var(--tw-inset-ring-color, currentColor)", important),
            new Declaration("box-shadow", "var(--tw-inset-shadow), var(--tw-inset-ring-shadow), var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow)", important));
    }

    private static ImmutableList<AstNode> CreateColorDeclarations(string value, bool important)
    {
        // Inset ring color sets the --tw-inset-ring-color variable
        return ImmutableList.Create<AstNode>(
            new Declaration("--tw-inset-ring-color", value, important));
    }

    /// <summary>
    /// Returns examples of inset ring utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("inset-ring", "Apply 1px inset ring with default color"),
            new("inset-ring-2", "Apply 2px inset ring"),
            new("inset-ring-4", "Apply 4px inset ring"),
            new("inset-ring-red-500", "Set inset ring color to red-500"),
            new("inset-ring-blue-600", "Set inset ring color to blue-600"),
            new("inset-ring-red-500/50", "Set inset ring color to red-500 with 50% opacity"),
            new("inset-ring-[3px]", "Apply inset ring with arbitrary width"),
        };

        return examples;
    }
}