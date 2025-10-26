using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling the color of text shadows.
/// </summary>
internal class TextShadowColorUtility : BaseColorUtility
{
    protected override string Pattern => "text-shadow";

    protected override string CssProperty => "--tw-text-shadow-color";

    protected override string[] ColorNamespaces => NamespaceResolver.TextShadowColorChain;

    /// <summary>
    /// Override to use raw theme values instead of CSS variables to match Tailwind CSS.
    /// </summary>
    protected override bool TryResolveColor(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? color)
    {
        color = null;

        // Handle special keywords with color-mix format
        if (value.Value == "transparent")
        {
            color = "color-mix(in oklab, transparent var(--tw-text-shadow-alpha, 100%))";
            return true;
        }

        if (value.Value == "current")
        {
            color = "color-mix(in oklab, currentColor var(--tw-text-shadow-alpha, 100%))";
            return true;
        }

        if (value.Value == "inherit")
        {
            color = "color-mix(in oklab, inherit var(--tw-text-shadow-alpha, 100%))";
            return true;
        }

        if (value.Value == "initial")
        {
            color = "initial";
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
    /// Implement to register the --tw-text-shadow-color CSS property.
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

        // Register the text shadow color property to match Tailwind CSS
        propertyRegistry.Register("--tw-text-shadow-color", "*", false, null);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}