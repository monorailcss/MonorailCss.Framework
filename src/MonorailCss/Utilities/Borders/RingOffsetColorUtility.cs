using MonorailCss.Ast;
using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Utility for ring-offset-color values.
/// Handles: ring-offset-red-500, ring-offset-blue-600, ring-offset-[#123456], ring-offset-transparent, ring-offset-current
/// CSS: Sets --tw-ring-offset-color CSS variable
/// Supports opacity modifiers: ring-offset-red-500/50.
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