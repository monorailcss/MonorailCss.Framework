using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling the color of text shadows (<c>text-shadow-red-500</c>,
/// <c>text-shadow-[#0088cc]</c>, <c>text-shadow-(color:--c)</c>, and the
/// <c>current</c>/<c>inherit</c>/<c>transparent</c>/<c>initial</c> keywords).
/// </summary>
/// <remarks>
/// Same shape as inset-shadow color: sets <c>--tw-text-shadow-color</c> with the fallback +
/// <c>@supports</c> <c>color-mix</c> pattern (theme colors resolve to <c>var(--color-*)</c> inside
/// the mix). The bare <c>text-shadow-none</c> reset is handled by <see cref="TextShadowUtility"/>.
/// </remarks>
internal class TextShadowColorUtility : BaseShadowColorUtility
{
    protected override string Pattern => "text-shadow";

    protected override string CssProperty => "--tw-text-shadow-color";

    protected override string AlphaVariable => "--tw-text-shadow-alpha";

    protected override string[] ColorNamespaces => NamespaceResolver.TextShadowColorChain;
}
