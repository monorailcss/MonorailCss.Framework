using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utility for perspective-origin values.
/// Handles: perspective-origin-center, perspective-origin-top, perspective-origin-top-right, etc.
/// CSS: perspective-origin property with keyword values.
/// </summary>
internal class PerspectiveOriginUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        new Dictionary<string, (string, string)>
        {
            ["perspective-origin-center"] = ("perspective-origin", "center"),
            ["perspective-origin-top"] = ("perspective-origin", "top"),
            ["perspective-origin-top-right"] = ("perspective-origin", "top right"),
            ["perspective-origin-right"] = ("perspective-origin", "right"),
            ["perspective-origin-bottom-right"] = ("perspective-origin", "bottom right"),
            ["perspective-origin-bottom"] = ("perspective-origin", "bottom"),
            ["perspective-origin-bottom-left"] = ("perspective-origin", "bottom left"),
            ["perspective-origin-left"] = ("perspective-origin", "left"),
            ["perspective-origin-top-left"] = ("perspective-origin", "top left"),
        }.ToImmutableDictionary();
}