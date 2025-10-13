using System.Diagnostics.CodeAnalysis;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling the accent color of form controls.
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