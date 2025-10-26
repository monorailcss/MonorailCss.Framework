using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.DataTypes;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling the color of box shadows.
/// </summary>
internal class ShadowColorUtility : BaseColorUtility
{
    protected override string Pattern => "shadow";

    protected override string CssProperty => "--tw-shadow-color";

    protected override string[] ColorNamespaces => NamespaceResolver.ShadowColorChain;

    /// <summary>
    /// Override to use raw theme values instead of CSS variables to match Tailwind CSS.
    /// </summary>
    protected override bool TryResolveColor(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? color)
    {
        color = null;

        // Handle special keywords directly
        if (value.Value == "transparent")
        {
            color = "transparent";
            return true;
        }

        if (value.Value == "current")
        {
            color = "currentcolor";
            return true;
        }

        if (value.Value == "inherit")
        {
            color = "inherit";
            return true;
        }

        // Handle arbitrary values
        if (value.Kind == ValueKind.Arbitrary)
        {
            color = value.Value;
            return true;
        }

        // Use ResolveValue to get raw OKLCH values instead of CSS variables
        var resolved = theme.ResolveValue(value.Value, ColorNamespaces);
        if (resolved == null)
        {
            return false;
        }

        color = resolved;
        return true;
    }

    /// <summary>
    /// Override to generate @supports block with color-mix function for shadow colors.
    /// </summary>
    public override bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != Pattern)
        {
            return false;
        }

        if (functionalUtility.Value == null)
        {
            return false;
        }

        // For arbitrary values, check if it's actually a color
        if (functionalUtility.Value.Kind == ValueKind.Arbitrary)
        {
            var inferredType = DataTypeInference.InferDataType(
                functionalUtility.Value.Value,
                [DataType.Color, DataType.Length, DataType.LineWidth, DataType.Number]);
            if (inferredType != DataType.Color)
            {
                return false;
            }
        }

        if (!TryResolveColor(functionalUtility.Value, theme, out var color))
        {
            return false;
        }

        // Create the fallback declaration
        var fallbackDeclaration = new Declaration(CssProperty, color, candidate.Important);

        // For named colors (not special keywords or arbitrary values), also generate @supports block
        if (functionalUtility.Value.Kind != ValueKind.Arbitrary &&
            color != "transparent" && color != "currentcolor" && color != "inherit")
        {
            // Build the CSS variable name (e.g., --color-red-500 from red-500)
            var colorVarName = $"--color-{functionalUtility.Value.Value}";

            // Create the color-mix declaration for inside @supports
            var colorMixValue = $"color-mix(in oklab, var({colorVarName}) var(--tw-shadow-alpha), transparent)";
            var colorMixDeclaration = new Declaration(CssProperty, colorMixValue, candidate.Important);

            // Create @supports rule with color-mix test
            var supportsRule = new AtRule(
                "supports",
                "(color: color-mix(in lab, red, red))",
                ImmutableList.Create<AstNode>(colorMixDeclaration));

            results = ImmutableList.Create<AstNode>(fallbackDeclaration, supportsRule);
        }
        else
        {
            // For special keywords and arbitrary values, just use the simple declaration
            results = ImmutableList.Create<AstNode>(fallbackDeclaration);
        }

        return true;
    }

    /// <summary>
    /// Implement to register the --tw-shadow-color CSS property.
    /// </summary>
    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        if (functionalUtility.Root != Pattern)
        {
            return false;
        }

        if (functionalUtility.Value == null)
        {
            return false;
        }

        // Register the shadow color property to match Tailwind CSS
        propertyRegistry.Register("--tw-shadow-color", "*", false, null);

        // Call the overridden TryCompile implementation
        return TryCompile(candidate, theme, out results);
    }
}