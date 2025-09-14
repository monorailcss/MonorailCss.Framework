using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask image utilities (mask-none).
/// Maps to the CSS mask-image property.
/// </summary>
internal class MaskImageUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "mask-none", ("mask-image", "none") },
        }.ToImmutableDictionary();
}