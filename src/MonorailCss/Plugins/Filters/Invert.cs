using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The invert filter plugin.
/// </summary>
public class Invert : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.Filter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "invert-0", "invert(0)" },
            { "invert", "invert(1)" },
        }.ToImmutableDictionary();
}