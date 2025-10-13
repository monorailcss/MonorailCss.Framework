using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Utilities for controlling the color of an element's outline.
/// </summary>
internal class OutlineColorUtility : BaseColorUtility
{
    protected override string Pattern => "outline";
    protected override string CssProperty => "outline-color";
    protected override string[] ColorNamespaces => NamespaceResolver.OutlineColorChain;
}