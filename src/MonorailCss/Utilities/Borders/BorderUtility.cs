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
/// Utilities for controlling the width and color of an element's borders.
/// </summary>
internal class BorderUtility : IUtility
{
    private static readonly string[] _functionalRoots = [
        "border", "border-x", "border-y", "border-t", "border-r",
        "border-b", "border-l", "border-s", "border-e"
    ];

    public UtilityPriority Priority => UtilityPriority.ConstrainedFunctional;

    public string[] GetNamespaces() => NamespaceResolver.BorderColorChain;

    public string[] GetFunctionalRoots() => _functionalRoots;

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        var pattern = functionalUtility.Root;
        if (!_functionalRoots.Contains(pattern))
        {
            return false;
        }

        // Handle bare patterns like "border", "border-x" (use default 1px width)
        if (functionalUtility.Value == null)
        {
            results = CreateWidthDeclarations(pattern, "1px", candidate.Important);
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
                NamespaceResolver.BorderColorChain,
                out var resolvedValue,
                out var inferredType))
            {
                switch (inferredType)
                {
                    case DataType.LineWidth:
                    case DataType.Length:
                        results = CreateWidthDeclarations(pattern, resolvedValue, candidate.Important);
                        return true;

                    default:
                        // Apply opacity modifier if present for colors
                        if (candidate.Modifier != null && inferredType == DataType.Color)
                        {
                            if (ValueResolver.TryResolveOpacity(candidate.Modifier, theme, out var opacity))
                            {
                                resolvedValue = $"color-mix(in oklab, {resolvedValue} {opacity}, transparent)";
                            }
                        }

                        results = CreateColorDeclarations(pattern, resolvedValue, candidate.Important);
                        return true;
                }
            }

            return false;
        }

        // For named values, try width first (for backward compatibility)
        if (TryResolveAsWidth(functionalUtility.Value, theme, out var widthValue))
        {
            results = CreateWidthDeclarations(pattern, widthValue, candidate.Important);
            return true;
        }

        if (TryResolveAsColor(functionalUtility.Value, theme, candidate.Modifier, out var colorValue))
        {
            results = CreateColorDeclarations(pattern, colorValue, candidate.Important);
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

        var pattern = functionalUtility.Root;
        if (!_functionalRoots.Contains(pattern))
        {
            return false;
        }

        // Handle bare patterns like "border", "border-x" (use default 1px width)
        if (functionalUtility.Value == null)
        {
            results = CreateWidthDeclarations(pattern, "1px", candidate.Important, propertyRegistry);
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
                NamespaceResolver.BorderColorChain,
                out var resolvedValue,
                out var inferredType))
            {
                switch (inferredType)
                {
                    case DataType.LineWidth:
                    case DataType.Length:
                        results = CreateWidthDeclarations(pattern, resolvedValue, candidate.Important, propertyRegistry);
                        return true;

                    default:
                        // Apply opacity modifier if present for colors
                        if (candidate.Modifier != null && inferredType == DataType.Color)
                        {
                            if (ValueResolver.TryResolveOpacity(candidate.Modifier, theme, out var opacity))
                            {
                                resolvedValue = $"color-mix(in oklab, {resolvedValue} {opacity}, transparent)";
                            }
                        }

                        results = CreateColorDeclarations(pattern, resolvedValue, candidate.Important);
                        return true;
                }
            }

            return false;
        }

        // For named values, try width first (for backward compatibility)
        if (TryResolveAsWidth(functionalUtility.Value, theme, out var widthValue))
        {
            results = CreateWidthDeclarations(pattern, widthValue, candidate.Important, propertyRegistry);
            return true;
        }

        if (TryResolveAsColor(functionalUtility.Value, theme, candidate.Modifier, out var colorValue))
        {
            results = CreateColorDeclarations(pattern, colorValue, candidate.Important);
            return true;
        }

        return false;
    }

    private static bool TryResolveAsWidth(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? result)
    {
        // Try to resolve as border width using the dedicated resolver
        return ValueResolver.TryResolveBorderWidth(value, theme, out result);
    }

    private bool TryResolveAsColor(CandidateValue value, Theme.Theme theme, Modifier? modifier, [NotNullWhen(true)] out string? result)
    {
        result = null;

        // First try to resolve the base color
        if (!ValueResolver.TryResolveColor(value, theme, NamespaceResolver.BorderColorChain, out var baseColor))
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

    private static ImmutableList<AstNode> CreateWidthDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "border":
                declarations.Add(new Declaration("border-width", value, important));
                break;
            case "border-x":
                declarations.Add(new Declaration("border-inline-width", value, important));
                break;
            case "border-y":
                declarations.Add(new Declaration("border-block-width", value, important));
                break;
            case "border-t":
                declarations.Add(new Declaration("border-top-width", value, important));
                break;
            case "border-r":
                declarations.Add(new Declaration("border-right-width", value, important));
                break;
            case "border-b":
                declarations.Add(new Declaration("border-bottom-width", value, important));
                break;
            case "border-l":
                declarations.Add(new Declaration("border-left-width", value, important));
                break;
            case "border-s":
                declarations.Add(new Declaration("border-inline-start-width", value, important));
                break;
            case "border-e":
                declarations.Add(new Declaration("border-inline-end-width", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }

    private static ImmutableList<AstNode> CreateWidthDeclarations(string pattern, string value, bool important, CssPropertyRegistry propertyRegistry)
    {
        // Register the border style property
        propertyRegistry.Register("--tw-border-style", "*", false, "solid");

        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "border":
                declarations.Add(new Declaration("border-style", "var(--tw-border-style)", important));
                declarations.Add(new Declaration("border-width", value, important));
                break;
            case "border-x":
                declarations.Add(new Declaration("border-inline-style", "var(--tw-border-style)", important));
                declarations.Add(new Declaration("border-inline-width", value, important));
                break;
            case "border-y":
                declarations.Add(new Declaration("border-block-style", "var(--tw-border-style)", important));
                declarations.Add(new Declaration("border-block-width", value, important));
                break;
            case "border-t":
                declarations.Add(new Declaration("border-top-style", "var(--tw-border-style)", important));
                declarations.Add(new Declaration("border-top-width", value, important));
                break;
            case "border-r":
                declarations.Add(new Declaration("border-right-style", "var(--tw-border-style)", important));
                declarations.Add(new Declaration("border-right-width", value, important));
                break;
            case "border-b":
                declarations.Add(new Declaration("border-bottom-style", "var(--tw-border-style)", important));
                declarations.Add(new Declaration("border-bottom-width", value, important));
                break;
            case "border-l":
                declarations.Add(new Declaration("border-left-style", "var(--tw-border-style)", important));
                declarations.Add(new Declaration("border-left-width", value, important));
                break;
            case "border-s":
                declarations.Add(new Declaration("border-inline-start-style", "var(--tw-border-style)", important));
                declarations.Add(new Declaration("border-inline-start-width", value, important));
                break;
            case "border-e":
                declarations.Add(new Declaration("border-inline-end-style", "var(--tw-border-style)", important));
                declarations.Add(new Declaration("border-inline-end-width", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }

    private static ImmutableList<AstNode> CreateColorDeclarations(string pattern, string value, bool important)
    {
        var declarations = new List<AstNode>();

        switch (pattern)
        {
            case "border":
                declarations.Add(new Declaration("border-color", value, important));
                break;
            case "border-x":
                declarations.Add(new Declaration("border-inline-color", value, important));
                break;
            case "border-y":
                declarations.Add(new Declaration("border-block-color", value, important));
                break;
            case "border-t":
                declarations.Add(new Declaration("border-top-color", value, important));
                break;
            case "border-r":
                declarations.Add(new Declaration("border-right-color", value, important));
                break;
            case "border-b":
                declarations.Add(new Declaration("border-bottom-color", value, important));
                break;
            case "border-l":
                declarations.Add(new Declaration("border-left-color", value, important));
                break;
            case "border-s":
                declarations.Add(new Declaration("border-inline-start-color", value, important));
                break;
            case "border-e":
                declarations.Add(new Declaration("border-inline-end-color", value, important));
                break;
        }

        return declarations.ToImmutableList();
    }

    /// <summary>
    /// Returns examples of border utilities.
    /// </summary>
    public IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        var examples = new List<Documentation.UtilityExample>
        {
            new("border", "Apply 1px border on all sides"),
            new("border-2", "Apply 2px border on all sides"),
            new("border-4", "Apply 4px border on all sides"),
            new("border-red-500", "Set border color to red-500"),
            new("border-blue-600", "Set border color to blue-600"),
            new("border-t-4", "Apply 4px border on top"),
            new("border-x-2", "Apply 2px border on left and right (horizontal)"),
            new("border-red-500/50", "Set border color to red-500 with 50% opacity"),
            new("border-[3px]", "Apply border with arbitrary width"),
        };

        return examples;
    }
}