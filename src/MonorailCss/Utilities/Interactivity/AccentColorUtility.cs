using System.Diagnostics.CodeAnalysis;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utility for accent-color values.
/// Handles: accent-auto, accent-inherit, accent-current, accent-transparent, accent-red-500, accent-blue-600/50, etc.
/// CSS: accent-color: auto, accent-color: inherit, accent-color: var(--color-red-500), etc.
/// </summary>
internal class AccentColorUtility : BaseColorUtility
{
    protected override string Pattern => "accent";

    protected override string CssProperty => "accent-color";

    protected override string[] ColorNamespaces => NamespaceResolver.AccentColorChain;

    protected override bool TryResolveColor(CandidateValue value, Theme.Theme theme, [NotNullWhen(true)] out string? color)
    {
        // Handle the static 'auto' value
        if (value.Value == "auto")
        {
            color = "auto";
            return true;
        }

        // Use base class resolution for all other color values
        return base.TryResolveColor(value, theme, out color);
    }
}