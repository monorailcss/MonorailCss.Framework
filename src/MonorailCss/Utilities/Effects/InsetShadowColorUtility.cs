using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling the color of inset shadows (<c>inset-shadow-red-500</c>,
/// <c>inset-shadow-[#0088cc]</c>, <c>inset-shadow-(color:--c)</c>, and the
/// <c>current</c>/<c>inherit</c>/<c>transparent</c>/<c>initial</c> keywords).
/// </summary>
internal class InsetShadowColorUtility : BaseShadowColorUtility
{
    protected override string Pattern => "inset-shadow";

    protected override string CssProperty => "--tw-inset-shadow-color";

    protected override string AlphaVariable => "--tw-inset-shadow-alpha";

    protected override string[] ColorNamespaces => NamespaceResolver.InsetShadowColorChain;
}
