using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Accessibility;

/// <summary>
/// Handles forced color adjust utilities (forced-color-adjust-auto, forced-color-adjust-none).
/// </summary>
internal class ForcedColorAdjustUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "forced-color-adjust-auto", ("forced-color-adjust", "auto") },
            { "forced-color-adjust-none", ("forced-color-adjust", "none") },
        }.ToImmutableDictionary();
}