using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Handles static accent-color utilities (accent-auto).
/// For color-based accent utilities, see AccentColorUtility.
/// </summary>
internal class AccentColorStaticUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "accent-auto", ("accent-color", "auto") },
        }.ToImmutableDictionary();
}