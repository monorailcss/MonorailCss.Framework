using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The backdrop-hue-rotate filter plugin.
/// </summary>
public class BackdropHueRotate : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.BackdropFilter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "backdrop-hue-rotate-0", "hue-rotate(0deg)" },
            { "backdrop-hue-rotate-15", "hue-rotate(15deg)" },
            { "backdrop-hue-rotate-30", "hue-rotate(30deg)" },
            { "backdrop-hue-rotate-60", "hue-rotate(60deg)" },
            { "backdrop-hue-rotate-90", "hue-rotate(90deg)" },
            { "backdrop-hue-rotate-180", "hue-rotate(180deg)" },
        }.ToImmutableDictionary();
}