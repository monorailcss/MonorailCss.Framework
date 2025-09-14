using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask type utilities (mask-type-alpha, mask-type-luminance).
/// Maps to the CSS mask-type property.
/// </summary>
internal class MaskTypeUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "mask-type-alpha", ("mask-type", "alpha") },
            { "mask-type-luminance", ("mask-type", "luminance") },
        }.ToImmutableDictionary();
}