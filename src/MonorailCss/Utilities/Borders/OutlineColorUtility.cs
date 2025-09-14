using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Handles outline-color utilities.
///
/// Color patterns: outline-red-500, outline-blue-600, outline-transparent, etc.
///
/// Maps to CSS outline-color property with opacity support.
/// </summary>
internal class OutlineColorUtility : BaseColorUtility
{
    protected override string Pattern => "outline";
    protected override string CssProperty => "outline-color";
    protected override string[] ColorNamespaces => NamespaceResolver.OutlineColorChain;
}