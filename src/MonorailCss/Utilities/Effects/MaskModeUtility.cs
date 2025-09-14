using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles mask mode utilities (mask-alpha, mask-luminance, mask-match).
/// Maps to the CSS mask-mode property.
/// </summary>
internal class MaskModeUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "mask-alpha", ("mask-mode", "alpha") },
            { "mask-luminance", ("mask-mode", "luminance") },
            { "mask-match", ("mask-mode", "match-source") },
        }.ToImmutableDictionary();
}