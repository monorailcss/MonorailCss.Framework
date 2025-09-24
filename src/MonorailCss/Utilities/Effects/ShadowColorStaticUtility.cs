using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles static shadow color utilities (shadow-current, shadow-inherit, shadow-transparent, shadow-initial).
/// </summary>
internal class ShadowColorStaticUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "shadow-current", ("--tw-shadow-color", "color-mix(in oklab, currentColor var(--tw-shadow-alpha), transparent)") },
            { "shadow-inherit", ("--tw-shadow-color", "color-mix(in oklab, inherit var(--tw-shadow-alpha), transparent)") },
            { "shadow-transparent", ("--tw-shadow-color", "color-mix(in oklab, transparent var(--tw-shadow-alpha), transparent)") },
            { "shadow-initial", ("--tw-shadow-color", "initial") },
        }.ToImmutableDictionary();
}