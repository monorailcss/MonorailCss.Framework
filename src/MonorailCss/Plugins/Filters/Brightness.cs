using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The brightness filter plugin.
/// </summary>
public class Brightness : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.Filter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "brightness-0", "brightness(0)" },
            { "brightness-50", "brightness(.5)" },
            { "brightness-75", "brightness(.75)" },
            { "brightness-90", "brightness(.9)" },
            { "brightness-95", "brightness(.95)" },
            { "brightness-100", "brightness(1)" },
            { "brightness-105", "brightness(1.05)" },
            { "brightness-110", "brightness(1.1)" },
            { "brightness-125", "brightness(1.25)" },
            { "brightness-150", "brightness(1.5)" },
            { "brightness-200", "brightness(2)" },
        }.ToImmutableDictionary();
}