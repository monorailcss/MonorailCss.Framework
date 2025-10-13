using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling the color of the text input cursor.
/// </summary>
internal class CaretColorUtility : BaseColorUtility
{
    protected override string Pattern => "caret";

    protected override string CssProperty => "caret-color";

    protected override string[] ColorNamespaces => NamespaceResolver.CaretColorChain;
}