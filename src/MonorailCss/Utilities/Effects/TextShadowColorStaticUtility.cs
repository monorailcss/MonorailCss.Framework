using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles static text shadow color utilities (text-shadow-current, text-shadow-inherit, text-shadow-transparent, text-shadow-initial).
/// </summary>
internal class TextShadowColorStaticUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "text-shadow-current", ("--tw-text-shadow-color", "color-mix(in oklab, currentColor var(--tw-text-shadow-alpha), transparent)") },
            { "text-shadow-inherit", ("--tw-text-shadow-color", "color-mix(in oklab, inherit var(--tw-text-shadow-alpha), transparent)") },
            { "text-shadow-transparent", ("--tw-text-shadow-color", "color-mix(in oklab, transparent var(--tw-text-shadow-alpha), transparent)") },
            { "text-shadow-initial", ("--tw-text-shadow-color", "initial") },
            { "text-shadow-none", ("text-shadow", "none") },
        }.ToImmutableDictionary();
}