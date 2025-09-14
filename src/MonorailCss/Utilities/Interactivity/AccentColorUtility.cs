using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utility for accent-color values.
/// Handles: accent-inherit, accent-current, accent-transparent, accent-red-500, accent-blue-600/50, etc.
/// CSS: accent-color: inherit, accent-color: var(--color-red-500), etc.
/// </summary>
internal class AccentColorUtility : BaseColorUtility
{
    protected override string Pattern => "accent";

    protected override string CssProperty => "accent-color";

    protected override string[] ColorNamespaces => NamespaceResolver.AccentColorChain;
}