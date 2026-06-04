using System.Collections.Immutable;
using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Utilities for controlling the color of drop shadow filters (<c>drop-shadow-red-500</c>,
/// <c>drop-shadow-[#0088cc]</c>, <c>drop-shadow-(color:--c)</c>, and the
/// <c>current</c>/<c>inherit</c>/<c>transparent</c>/<c>initial</c> keywords).
/// </summary>
/// <remarks>
/// Unlike box-shadow / inset-shadow, applying a drop-shadow color also re-points
/// <c>--tw-drop-shadow</c> at <c>--tw-drop-shadow-size</c> (the color-injected layer), since a sized
/// <c>drop-shadow</c> otherwise references the un-injected theme value directly.
/// </remarks>
internal class DropShadowColorUtility : BaseShadowColorUtility
{
    protected override string Pattern => "drop-shadow";

    protected override string CssProperty => "--tw-drop-shadow-color";

    protected override string AlphaVariable => "--tw-drop-shadow-alpha";

    protected override string[] ColorNamespaces => NamespaceResolver.DropShadowColorChain;

    protected override IEnumerable<AstNode> ExtraDeclarations(bool important) =>
        ImmutableList.Create<AstNode>(new Declaration("--tw-drop-shadow", "var(--tw-drop-shadow-size)", important));
}
