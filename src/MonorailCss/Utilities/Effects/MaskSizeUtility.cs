using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask size utilities (mask-auto, mask-cover, mask-contain).
/// Maps to the CSS mask-size property.
/// </summary>
internal class MaskSizeUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "mask-auto", ("mask-size", "auto") },
            { "mask-cover", ("mask-size", "cover") },
            { "mask-contain", ("mask-size", "contain") },
        }.ToImmutableDictionary();
}