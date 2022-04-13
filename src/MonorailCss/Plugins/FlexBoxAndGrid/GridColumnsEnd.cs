using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The grid-template-columns plugin.
/// </summary>
public class GridColumnsEnd : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "grid-column-end";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        var dict = new Dictionary<string, string>();

        for (var i = 1; i <= 13; i++)
        {
            dict.Add($"col-end-{i}", i.ToString());
        }

        dict.Add("col-end-auto", "auto");
        return dict.ToImmutableDictionary();
    }
}