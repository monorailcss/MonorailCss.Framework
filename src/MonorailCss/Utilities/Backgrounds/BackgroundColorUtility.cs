using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Handles background-color utilities (bg-*).
/// </summary>
internal class BackgroundColorUtility : BaseColorUtility
{
    protected override string Pattern => "bg";
    protected override string CssProperty => "background-color";
    protected override string[] ColorNamespaces => NamespaceResolver.BackgroundColorChain;
}