using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The sepia filter plugin.
/// </summary>
public class Sepia : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.Filter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "sepia-0", "sepia(0)" },
            { "sepia", "sepia(1)" },
        }.ToImmutableDictionary();
}