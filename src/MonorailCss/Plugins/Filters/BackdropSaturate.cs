using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The backdrop-saturate filter plugin.
/// </summary>
public class BackdropSaturate : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.BackdropFilter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "backdrop-saturate-0", "saturate(0)" },
            { "backdrop-saturate-50", "saturate(.5)" },
            { "backdrop-saturate-100", "saturate(1)" },
            { "backdrop-saturate-150", "saturate(1.5)" },
            { "backdrop-saturate-200", "saturate(2)" },
        }.ToImmutableDictionary();
}