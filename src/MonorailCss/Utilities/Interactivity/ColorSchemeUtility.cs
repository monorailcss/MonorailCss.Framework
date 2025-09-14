using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Handles color-scheme utilities (scheme-normal, scheme-dark, scheme-light, etc.).
/// </summary>
internal class ColorSchemeUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "scheme-normal", ("color-scheme", "normal") },
            { "scheme-dark", ("color-scheme", "dark") },
            { "scheme-light", ("color-scheme", "light") },
            { "scheme-light-dark", ("color-scheme", "light dark") },
            { "scheme-only-dark", ("color-scheme", "only dark") },
            { "scheme-only-light", ("color-scheme", "only light") },
        }.ToImmutableDictionary();
}