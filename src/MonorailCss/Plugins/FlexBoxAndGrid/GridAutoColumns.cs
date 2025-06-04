using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The grid-auto-columns plugin.
/// </summary>
public class GridAutoColumns : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "grid-auto-columns";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        var dict = new Dictionary<string, string>
        {
            ["auto-cols-auto"] = "auto",
            ["auto-cols-min"] = "min-content",
            ["auto-cols-max"] = "max-content",
            ["auto-cols-fr"] = "minmax(0, 1fr)",
        };

        return dict.ToImmutableDictionary();
    }
}
