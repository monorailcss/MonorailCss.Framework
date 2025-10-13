using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utilities for controlling the perspective of 3D transformed elements.
/// </summary>
internal class PerspectiveStaticUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        new Dictionary<string, (string, string)>
        {
            ["perspective-none"] = ("perspective", "none"),
        }.ToImmutableDictionary();
}