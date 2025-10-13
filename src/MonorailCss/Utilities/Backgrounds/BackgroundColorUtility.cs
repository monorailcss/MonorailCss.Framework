using MonorailCss.Core;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utilities for controlling the background color of an element.
/// </summary>
internal class BackgroundColorUtility : BaseColorUtility
{
    protected override string Pattern => "bg";
    protected override string CssProperty => "background-color";
    protected override string[] ColorNamespaces => NamespaceResolver.BackgroundColorChain;

    public Documentation.UtilityMetadata GetMetadata()
    {
        return new Documentation.UtilityMetadata(
            "BackgroundColorUtility",
            "Backgrounds",
            "Sets the background color of an element",
            supportsModifiers: true,
            supportsArbitraryValues: true);
    }
}