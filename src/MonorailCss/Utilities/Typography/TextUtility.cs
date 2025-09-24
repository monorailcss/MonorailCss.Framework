using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.DataTypes;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Unified text utility that handles both font-size and text color, matching Tailwind's behavior.
/// Handles: text-{color}, text-{size}, text-[arbitrary]
/// CSS: color for colors, font-size (and optionally line-height) for sizes.
/// </summary>
internal class TextUtility : IUtility
{
    // Higher priority than normal functional utilities to ensure proper resolution
    public UtilityPriority Priority => UtilityPriority.NamespaceHandler;

    public bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != "text")
        {
            return false;
        }

        if (functionalUtility.Value == null)
        {
            return false;
        }

        var value = functionalUtility.Value.Value;

        // Handle arbitrary values
        if (functionalUtility.Value.Kind == ValueKind.Arbitrary)
        {
            var inferredType = DataTypeInference.InferDataType(
                value,
                [DataType.Color, DataType.Length, DataType.Percentage, DataType.AbsoluteSize, DataType.RelativeSize]);

            switch (inferredType)
            {
                case DataType.Length:
                case DataType.Percentage:
                case DataType.AbsoluteSize:
                case DataType.RelativeSize:
                    // It's a font-size
                    results = GenerateFontSizeDeclarations(value, candidate.Modifier, theme, candidate.Important);
                    return true;

                case DataType.Color:
                    // It's a color
                    if (TryApplyColorWithModifier(value, candidate.Modifier, theme, out var color))
                    {
                        results = ImmutableList.Create<AstNode>(
                            new Declaration("color", color, candidate.Important));
                        return true;
                    }

                    return false;
                default:
                    return false;
            }
        }

        // For named values, try color first (as Tailwind does)

        // Try as color
        if (TryResolveAsColor(functionalUtility.Value, theme, out var resolvedColor))
        {
            if (TryApplyColorWithModifier(resolvedColor, candidate.Modifier, theme, out var finalColor))
            {
                results = ImmutableList.Create<AstNode>(
                    new Declaration("color", finalColor, candidate.Important));
                return true;
            }
        }

        // Try as font-size
        if (TryResolveAsFontSize(functionalUtility.Value, theme, out var fontSize, out var lineHeight))
        {
            results = GenerateFontSizeDeclarations(fontSize, lineHeight, candidate.Modifier, theme, candidate.Important);
            return true;
        }

        return false;
    }

    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Delegate to the main TryCompile method
        return TryCompile(candidate, theme, out results);
    }

    public string[] GetNamespaces()
    {
        // Return all namespaces this utility uses
        return NamespaceResolver.AppendFallbacks(NamespaceResolver.TextColorChain, "--font-size", "--text");
    }

    private bool TryResolveAsColor(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? color)
    {
        // Special color keywords
        if (value.Value is "inherit" or "current" or "transparent")
        {
            color = value.Value == "current" ? "currentColor" : value.Value;
            return true;
        }

        // Try to resolve from theme
        return ValueResolver.TryResolveColor(value, theme, NamespaceResolver.TextColorChain, out color);
    }

    private bool TryResolveAsFontSize(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? fontSize, out string? lineHeight)
    {
        fontSize = null;
        lineHeight = null;

        // Check for known font-size values
        var knownSizes = new Dictionary<string, (string Size, string LineHeight)>
        {
            ["xs"] = ("0.75rem", "1rem"),
            ["sm"] = ("0.875rem", "1.25rem"),
            ["base"] = ("1rem", "1.5rem"),
            ["lg"] = ("1.125rem", "1.75rem"),
            ["xl"] = ("1.25rem", "1.75rem"),
            ["2xl"] = ("1.5rem", "2rem"),
            ["3xl"] = ("1.875rem", "2.25rem"),
            ["4xl"] = ("2.25rem", "2.5rem"),
            ["5xl"] = ("3rem", "1"),
            ["6xl"] = ("3.75rem", "1"),
            ["7xl"] = ("4.5rem", "1"),
            ["8xl"] = ("6rem", "1"),
            ["9xl"] = ("8rem", "1"),
        };

        if (knownSizes.TryGetValue(value.Value, out _))
        {
            // Use CSS variable for font-size
            var fontSizeKey = $"--text-{value.Value}";
            fontSize = $"var({fontSizeKey})";

            // Check if theme has a line-height variable for this size
            var lineHeightKey = $"--text-{value.Value}--line-height";
            var themeLineHeight = theme.ResolveValue(lineHeightKey, []);
            if (themeLineHeight != null)
            {
                // Use nested var pattern for line-height like Tailwind does
                lineHeight = $"var(--tw-leading, var({lineHeightKey}))";

                // ThemeVariableTrackingStage automatically tracks var() references
            }
            else
            {
                // Fall back to hardcoded line-height with CSS variable pattern
                lineHeight = $"var(--tw-leading, var({lineHeightKey}))";
            }

            return true;
        }

        // Check if it can be resolved from theme's --text namespace
        var resolvedValue = theme.ResolveValue($"--text-{value.Value}", []);
        if (resolvedValue != null)
        {
            fontSize = $"var(--text-{value.Value})";

            // ThemeVariableTrackingStage automatically tracks var() references

            // Check for associated line-height
            var lineHeightKey = $"--text-{value.Value}--line-height";
            var themeLineHeight = theme.ResolveValue(lineHeightKey, []);
            if (themeLineHeight != null)
            {
                // Use nested var pattern for line-height like Tailwind does
                lineHeight = $"var(--tw-leading, var({lineHeightKey}))";

                // ThemeVariableTrackingStage automatically tracks var() references
            }

            return true;
        }

        return false;
    }

    private ImmutableList<AstNode> GenerateFontSizeDeclarations(string fontSize, Modifier? modifier, Theme.Theme theme, bool important)
    {
        var declarations = ImmutableList.CreateBuilder<AstNode>();
        declarations.Add(new Declaration("font-size", fontSize, important));

        // Handle line-height modifier if present
        if (modifier != null)
        {
            string? lineHeightValue = null;

            if (modifier.Kind == ModifierKind.Arbitrary)
            {
                lineHeightValue = modifier.Value;
            }
            else if (modifier.Kind == ModifierKind.Named)
            {
                // Try to resolve from theme
                var resolved = theme.ResolveValue($"--leading-{modifier.Value}", []);
                if (resolved != null)
                {
                    lineHeightValue = $"var(--leading-{modifier.Value})";

                    // ThemeVariableTrackingStage automatically tracks var() references
                }
                else if (modifier.Value == "none")
                {
                    lineHeightValue = "1";
                }
                else if (IsValidSpacingMultiplier(modifier.Value))
                {
                    var multiplier = theme.ResolveValue("--spacing", []);
                    if (multiplier != null)
                    {
                        lineHeightValue = $"calc({multiplier} * {modifier.Value})";
                    }
                }
            }

            if (lineHeightValue != null)
            {
                declarations.Add(new Declaration("line-height", lineHeightValue, important));
            }
        }

        return declarations.ToImmutable();
    }

    private ImmutableList<AstNode> GenerateFontSizeDeclarations(string fontSize, string? lineHeight, Modifier? modifier, Theme.Theme theme, bool important)
    {
        // If there's a modifier, it overrides the default line-height
        if (modifier != null)
        {
            return GenerateFontSizeDeclarations(fontSize, modifier, theme, important);
        }

        var declarations = ImmutableList.CreateBuilder<AstNode>();
        declarations.Add(new Declaration("font-size", fontSize, important));

        // Add line-height if it exists
        if (!string.IsNullOrEmpty(lineHeight))
        {
            declarations.Add(new Declaration("line-height", lineHeight, important));
        }

        return declarations.ToImmutable();
    }

    private bool TryApplyColorWithModifier(string color, Modifier? modifier, Theme.Theme theme, out string result)
    {
        result = color;

        if (modifier == null)
        {
            return true;
        }

        // Only apply opacity modifiers
        if (modifier.Value == "negative")
        {
            return true;
        }

        if (!ValueResolver.TryResolveOpacity(modifier, theme, out var opacity))
        {
            return true; // Return the color without opacity if we can't resolve it
        }

        // Use color-mix() to apply opacity
        result = $"color-mix(in oklab, {color} {opacity}, transparent)";
        return true;
    }

    private bool IsValidSpacingMultiplier(string value)
    {
        // Check if it's a valid spacing multiplier (positive integer or decimal)
        if (double.TryParse(value, out var num))
        {
            return num >= 0;
        }

        return false;
    }
}