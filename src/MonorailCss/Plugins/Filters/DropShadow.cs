using System.Collections.Immutable;

using MonorailCss.Css;
namespace MonorailCss.Plugins.Filters;

/// <summary>
/// The drop-shadow filter plugin.
/// </summary>
public class DropShadow : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => CssProperties.Filter;

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "drop-shadow-sm", "drop-shadow(0 1px 1px rgb(0 0 0 / 0.05))" },
            { "drop-shadow", "drop-shadow(0 1px 2px rgb(0 0 0 / 0.1)) drop-shadow(0 1px 1px rgb(0 0 0 / 0.06))" },
            { "drop-shadow-md", "drop-shadow(0 4px 3px rgb(0 0 0 / 0.07)) drop-shadow(0 2px 2px rgb(0 0 0 / 0.06))" },
            { "drop-shadow-lg", "drop-shadow(0 10px 8px rgb(0 0 0 / 0.04)) drop-shadow(0 4px 3px rgb(0 0 0 / 0.1))" },
            { "drop-shadow-xl", "drop-shadow(0 20px 13px rgb(0 0 0 / 0.03)) drop-shadow(0 8px 5px rgb(0 0 0 / 0.08))" },
            { "drop-shadow-2xl", "drop-shadow(0 25px 25px rgb(0 0 0 / 0.15))" },
            { "drop-shadow-none", "drop-shadow(0 0 #0000)" },
        }.ToImmutableDictionary();
}