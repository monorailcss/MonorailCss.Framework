using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The grid-template-columns plugin.
/// </summary>
public class GridRows : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "grid-template-rows";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        var dict = new Dictionary<string, string>();

        for (var i = 1; i <= 12; i++)
        {
            dict.Add($"grid-rows-{i}", $"repeat({i}, minmax(0, 1fr))");
        }

        dict.Add("grid-cols-none", "none");
        return dict.ToImmutableDictionary();
    }
}