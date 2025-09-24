using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Effects;

/// <summary>
/// Handles inset shadow color utilities.
/// </summary>
internal class InsetShadowColorUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "inset-shadow-current", ("--tw-inset-shadow-color", "color-mix(in oklab, currentColor var(--tw-inset-shadow-alpha), transparent)") },
            { "inset-shadow-inherit", ("--tw-inset-shadow-color", "color-mix(in oklab, inherit var(--tw-inset-shadow-alpha), transparent)") },
            { "inset-shadow-transparent", ("--tw-inset-shadow-color", "color-mix(in oklab, transparent var(--tw-inset-shadow-alpha), transparent)") },
            { "inset-shadow-initial", ("--tw-inset-shadow-color", "initial") },
        }.ToImmutableDictionary();
}