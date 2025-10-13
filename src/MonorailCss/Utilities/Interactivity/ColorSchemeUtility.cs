using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling the preferred color scheme of an element.
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