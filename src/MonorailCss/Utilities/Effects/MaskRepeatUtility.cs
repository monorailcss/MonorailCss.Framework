using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask repeat utilities (mask-repeat, mask-no-repeat, mask-repeat-x, etc.).
/// Maps to the CSS mask-repeat property.
/// </summary>
internal class MaskRepeatUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "mask-repeat", ("mask-repeat", "repeat") },
            { "mask-no-repeat", ("mask-repeat", "no-repeat") },
            { "mask-repeat-x", ("mask-repeat", "repeat-x") },
            { "mask-repeat-y", ("mask-repeat", "repeat-y") },
            { "mask-repeat-round", ("mask-repeat", "round") },
            { "mask-repeat-space", ("mask-repeat", "space") },
        }.ToImmutableDictionary();
}