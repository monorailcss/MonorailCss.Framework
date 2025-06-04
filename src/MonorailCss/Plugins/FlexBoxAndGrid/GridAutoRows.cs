using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The grid-auto-rows plugin.
/// </summary>
public class GridAutoRows : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "grid-auto-rows";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        var dict = new Dictionary<string, string>
        {
            ["auto-rows-auto"] = "auto",
            ["auto-rows-min"] = "min-content",
            ["auto-rows-max"] = "max-content",
            ["auto-rows-fr"] = "minmax(0, 1fr)",
        };

        return dict.ToImmutableDictionary();
    }
}
