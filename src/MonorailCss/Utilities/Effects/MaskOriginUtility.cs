using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask origin utilities (mask-origin-border, mask-origin-padding, mask-origin-content).
/// Maps to the CSS mask-origin property.
/// </summary>
internal class MaskOriginUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "mask-origin-border", ("mask-origin", "border-box") },
            { "mask-origin-padding", ("mask-origin", "padding-box") },
            { "mask-origin-content", ("mask-origin", "content-box") },
        }.ToImmutableDictionary();
}