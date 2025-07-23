using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The backdrop-opacity filter plugin.
/// </summary>
public class BackdropOpacity : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.BackdropFilter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "backdrop-opacity-0", "opacity(0)" },
            { "backdrop-opacity-5", "opacity(0.05)" },
            { "backdrop-opacity-10", "opacity(0.1)" },
            { "backdrop-opacity-20", "opacity(0.2)" },
            { "backdrop-opacity-25", "opacity(0.25)" },
            { "backdrop-opacity-30", "opacity(0.3)" },
            { "backdrop-opacity-40", "opacity(0.4)" },
            { "backdrop-opacity-50", "opacity(0.5)" },
            { "backdrop-opacity-60", "opacity(0.6)" },
            { "backdrop-opacity-70", "opacity(0.7)" },
            { "backdrop-opacity-75", "opacity(0.75)" },
            { "backdrop-opacity-80", "opacity(0.8)" },
            { "backdrop-opacity-90", "opacity(0.9)" },
            { "backdrop-opacity-95", "opacity(0.95)" },
            { "backdrop-opacity-100", "opacity(1)" },
        }.ToImmutableDictionary();
}