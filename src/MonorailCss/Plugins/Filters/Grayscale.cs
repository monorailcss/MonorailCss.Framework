using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The grayscale filter plugin.
/// </summary>
public class Grayscale : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.Filter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "grayscale-0", "grayscale(0)" },
            { "grayscale", "grayscale(1)" },
        }.ToImmutableDictionary();
}