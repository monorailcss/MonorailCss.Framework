using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask radial gradient position utilities.
/// Handles: mask-radial-at-top, mask-radial-at-bottom, mask-radial-at-center, etc.
/// </summary>
internal class MaskRadialPositionUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "mask-radial-at-center", ("--tw-mask-radial-position", "center") },
            { "mask-radial-at-top", ("--tw-mask-radial-position", "top") },
            { "mask-radial-at-bottom", ("--tw-mask-radial-position", "bottom") },
            { "mask-radial-at-left", ("--tw-mask-radial-position", "left") },
            { "mask-radial-at-right", ("--tw-mask-radial-position", "right") },
            { "mask-radial-at-top-left", ("--tw-mask-radial-position", "top left") },
            { "mask-radial-at-top-right", ("--tw-mask-radial-position", "top right") },
            { "mask-radial-at-bottom-left", ("--tw-mask-radial-position", "bottom left") },
            { "mask-radial-at-bottom-right", ("--tw-mask-radial-position", "bottom right") },
        }.ToImmutableDictionary();
}