using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask position utilities (mask-top, mask-center, mask-bottom-right, etc.).
/// Maps to the CSS mask-position property.
/// </summary>
internal class MaskPositionUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "mask-top-left", ("mask-position", "left top") },
            { "mask-top", ("mask-position", "top") },
            { "mask-top-right", ("mask-position", "right top") },
            { "mask-left", ("mask-position", "left") },
            { "mask-center", ("mask-position", "center") },
            { "mask-right", ("mask-position", "right") },
            { "mask-bottom-left", ("mask-position", "left bottom") },
            { "mask-bottom", ("mask-position", "bottom") },
            { "mask-bottom-right", ("mask-position", "right bottom") },
        }.ToImmutableDictionary();
}