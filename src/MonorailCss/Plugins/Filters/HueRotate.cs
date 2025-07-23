using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The hue-rotate filter plugin.
/// </summary>
public class HueRotate : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.Filter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "hue-rotate-0", "hue-rotate(0deg)" },
            { "hue-rotate-15", "hue-rotate(15deg)" },
            { "hue-rotate-30", "hue-rotate(30deg)" },
            { "hue-rotate-60", "hue-rotate(60deg)" },
            { "hue-rotate-90", "hue-rotate(90deg)" },
            { "hue-rotate-180", "hue-rotate(180deg)" },
        }.ToImmutableDictionary();
}