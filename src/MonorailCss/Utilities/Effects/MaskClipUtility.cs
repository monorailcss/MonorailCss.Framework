using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask clip utilities (mask-clip-border, mask-clip-padding, mask-clip-content).
/// Maps to the CSS mask-clip property.
/// </summary>
internal class MaskClipUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "mask-clip-border", ("mask-clip", "border-box") },
            { "mask-clip-padding", ("mask-clip", "padding-box") },
            { "mask-clip-content", ("mask-clip", "content-box") },
            { "mask-clip-fill", ("mask-clip", "fill-box") },
            { "mask-clip-stroke", ("mask-clip", "stroke-box") },
            { "mask-clip-view", ("mask-clip", "view-box") },
            { "mask-no-clip", ("mask-clip", "no-clip") },
        }.ToImmutableDictionary();
}