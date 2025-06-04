using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The grid-auto-flow plugin.
/// </summary>
public class GridAutoFlow : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "grid-auto-flow";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        var dict = new Dictionary<string, string>
        {
            ["grid-flow-row"] = "row",
            ["grid-flow-col"] = "column",
            ["grid-flow-row-dense"] = "row dense",
            ["grid-flow-col-dense"] = "column dense",
        };

        return dict.ToImmutableDictionary();
    }
}
