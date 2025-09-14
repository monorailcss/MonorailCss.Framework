using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utility for caret-color values.
/// Handles: caret-inherit, caret-current, caret-transparent, caret-red-500, caret-blue-600/50, etc.
/// CSS: caret-color: inherit, caret-color: var(--color-red-500), etc.
/// </summary>
internal class CaretColorUtility : BaseColorUtility
{
    protected override string Pattern => "caret";

    protected override string CssProperty => "caret-color";

    protected override string[] ColorNamespaces => NamespaceResolver.CaretColorChain;
}