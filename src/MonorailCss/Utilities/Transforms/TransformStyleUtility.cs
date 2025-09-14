using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utility for transform-style values.
/// Handles: transform-flat, transform-3d
/// CSS: transform-style property with flat and preserve-3d values.
/// </summary>
internal class TransformStyleUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        new Dictionary<string, (string, string)>
        {
            ["transform-flat"] = ("transform-style", "flat"),
            ["transform-3d"] = ("transform-style", "preserve-3d"),
        }.ToImmutableDictionary();
}