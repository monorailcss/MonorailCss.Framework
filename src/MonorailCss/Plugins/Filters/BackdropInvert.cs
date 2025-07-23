using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The backdrop-invert filter plugin.
/// </summary>
public class BackdropInvert : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.BackdropFilter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "backdrop-invert-0", "invert(0)" },
            { "backdrop-invert", "invert(1)" },
        }.ToImmutableDictionary();
}