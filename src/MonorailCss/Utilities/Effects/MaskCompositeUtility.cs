using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask composite utilities (mask-add, mask-subtract, mask-intersect, mask-exclude).
/// Maps to the CSS mask-composite property.
/// </summary>
internal class MaskCompositeUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "mask-add", ("mask-composite", "add") },
            { "mask-subtract", ("mask-composite", "subtract") },
            { "mask-intersect", ("mask-composite", "intersect") },
            { "mask-exclude", ("mask-composite", "exclude") },
        }.ToImmutableDictionary();
}