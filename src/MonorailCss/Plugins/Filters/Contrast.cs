using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The contrast filter plugin.
/// </summary>
public class Contrast : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.Filter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "contrast-0", "contrast(0)" },
            { "contrast-50", "contrast(.5)" },
            { "contrast-75", "contrast(.75)" },
            { "contrast-100", "contrast(1)" },
            { "contrast-125", "contrast(1.25)" },
            { "contrast-150", "contrast(1.5)" },
            { "contrast-200", "contrast(2)" },
        }.ToImmutableDictionary();
}