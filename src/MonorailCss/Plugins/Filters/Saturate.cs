using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The saturate filter plugin.
/// </summary>
public class Saturate : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.Filter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "saturate-0", "saturate(0)" },
            { "saturate-50", "saturate(.5)" },
            { "saturate-100", "saturate(1)" },
            { "saturate-150", "saturate(1.5)" },
            { "saturate-200", "saturate(2)" },
        }.ToImmutableDictionary();
}