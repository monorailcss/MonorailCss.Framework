using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The backdrop-grayscale filter plugin.
/// </summary>
public class BackdropGrayscale : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.BackdropFilter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "backdrop-grayscale-0", "grayscale(0)" },
            { "backdrop-grayscale", "grayscale(1)" },
        }.ToImmutableDictionary();
}