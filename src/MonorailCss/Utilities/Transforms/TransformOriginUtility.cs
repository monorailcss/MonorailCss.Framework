using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utility for transform-origin values.
/// Handles: origin-center, origin-top, origin-top-right, origin-right, origin-bottom-right, origin-bottom, origin-bottom-left, origin-left, origin-top-left, origin-*
/// CSS: transform-origin property with keyword values.
/// </summary>
internal class TransformOriginUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        new Dictionary<string, (string, string)>
        {
            ["origin-center"] = ("transform-origin", "center"),
            ["origin-top"] = ("transform-origin", "top"),
            ["origin-top-right"] = ("transform-origin", "top right"),
            ["origin-right"] = ("transform-origin", "right"),
            ["origin-bottom-right"] = ("transform-origin", "bottom right"),
            ["origin-bottom"] = ("transform-origin", "bottom"),
            ["origin-bottom-left"] = ("transform-origin", "bottom left"),
            ["origin-left"] = ("transform-origin", "left"),
            ["origin-top-left"] = ("transform-origin", "top left"),
        }.ToImmutableDictionary();
}