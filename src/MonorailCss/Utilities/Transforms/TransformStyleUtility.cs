using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utilities for controlling how child elements are rendered in 3D space.
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