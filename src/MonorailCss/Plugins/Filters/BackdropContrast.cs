using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The backdrop-contrast filter plugin.
/// </summary>
public class BackdropContrast : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.BackdropFilter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "backdrop-contrast-0", "contrast(0)" },
            { "backdrop-contrast-50", "contrast(.5)" },
            { "backdrop-contrast-75", "contrast(.75)" },
            { "backdrop-contrast-100", "contrast(1)" },
            { "backdrop-contrast-125", "contrast(1.25)" },
            { "backdrop-contrast-150", "contrast(1.5)" },
            { "backdrop-contrast-200", "contrast(2)" },
        }.ToImmutableDictionary();
}