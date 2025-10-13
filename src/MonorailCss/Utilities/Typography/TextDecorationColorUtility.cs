using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the color of text decorations.
/// </summary>
internal class TextDecorationColorUtility : BaseColorUtility
{
    protected override string Pattern => "decoration";

    protected override string CssProperty => "text-decoration-color";

    protected override string[] ColorNamespaces => NamespaceResolver.DecorationColorChain;
}