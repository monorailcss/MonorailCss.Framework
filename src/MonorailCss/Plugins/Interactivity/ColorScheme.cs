using System.Collections.Immutable;

namespace MonorailCss.Plugins.Interactivity;

/// <summary>
/// The color scheme plugin.
/// </summary>
public class ColorScheme : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "color-scheme";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        return new Dictionary<string, string>
        {
            { "scheme-normal",  "normal" },
            { "scheme-dark",  "dark" },
            { "scheme-light",  "light" },
            { "scheme-light-dark", "light dark" },
            { "scheme-only-dark", "only dark" },
            { "scheme-only-light", "only light;" },
        }.ToImmutableDictionary();
    }
}