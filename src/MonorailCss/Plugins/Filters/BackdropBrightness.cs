using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The backdrop-brightness filter plugin.
/// </summary>
public class BackdropBrightness : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.BackdropFilter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "backdrop-brightness-0", "brightness(0)" },
            { "backdrop-brightness-50", "brightness(.5)" },
            { "backdrop-brightness-75", "brightness(.75)" },
            { "backdrop-brightness-90", "brightness(.9)" },
            { "backdrop-brightness-95", "brightness(.95)" },
            { "backdrop-brightness-100", "brightness(1)" },
            { "backdrop-brightness-105", "brightness(1.05)" },
            { "backdrop-brightness-110", "brightness(1.1)" },
            { "backdrop-brightness-125", "brightness(1.25)" },
            { "backdrop-brightness-150", "brightness(1.5)" },
            { "backdrop-brightness-200", "brightness(2)" },
        }.ToImmutableDictionary();
}