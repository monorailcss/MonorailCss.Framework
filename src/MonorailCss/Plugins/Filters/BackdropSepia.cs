using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The backdrop-sepia filter plugin.
/// </summary>
public class BackdropSepia : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.BackdropFilter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "backdrop-sepia-0", "sepia(0)" },
            { "backdrop-sepia", "sepia(1)" },
        }.ToImmutableDictionary();
}