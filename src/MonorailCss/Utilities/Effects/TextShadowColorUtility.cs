using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Css;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utility for text shadow color values.
/// Handles: text-shadow-inherit, text-shadow-current, text-shadow-transparent, text-shadow-red-500, text-shadow-blue-600/50, etc.
/// CSS: --tw-text-shadow-color: inherit, --tw-text-shadow-color: oklch(63.7% 0.237 25.331), etc.
/// Note: Uses raw OKLCH values to match Tailwind CSS output, not CSS variables.
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
    /// Implement to register the --tw-text-shadow-color CSS property.
    /// </summary>
    public bool TryCompile(Candidate candidate, Theme.Theme theme, CssPropertyRegistry propertyRegistry, out ImmutableList<AstNode>? results)
    {
        // Register the text shadow color property to match Tailwind CSS
        propertyRegistry.Register("--tw-text-shadow-color", "*", false, string.Empty);

        // Call the base implementation
        return TryCompile(candidate, theme, out results);
    }
}