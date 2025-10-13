using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Utilities for controlling the color of ring offset shadows.
/// </summary>
internal class RingOffsetColorUtility : BaseColorUtility
{
    protected override string Pattern => "ring-offset";
    protected override string CssProperty => "--tw-ring-offset-color";
    protected override string[] ColorNamespaces => NamespaceResolver.RingOffsetColorChain;

    protected override Declaration CreateDeclaration(string color, bool important)
    {
        // Ring offset color sets the --tw-ring-offset-color variable
        return new Declaration("--tw-ring-offset-color", color, important);
    }
}