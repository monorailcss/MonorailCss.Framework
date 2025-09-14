using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utility for text-decoration-color values.
/// Handles: decoration-inherit, decoration-current, decoration-transparent, decoration-red-500, decoration-blue-600/50, etc.
/// CSS: text-decoration-color: inherit, text-decoration-color: var(--color-red-500), etc.
/// </summary>
internal class TextDecorationColorUtility : BaseColorUtility
{
    protected override string Pattern => "decoration";

    protected override string CssProperty => "text-decoration-color";

    protected override string[] ColorNamespaces => NamespaceResolver.DecorationColorChain;
}