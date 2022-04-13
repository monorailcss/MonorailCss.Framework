using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The grid-template-columns plugin.
/// </summary>
public class GridColumnSpan : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "grid-column";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        var dict = new Dictionary<string, string>();

        for (var i = 1; i <= 12; i++)
        {
            dict.Add($"col-span-{i}", $"span {i} / span {i}");
        }

        dict.Add("col-auto", "auto");
        dict.Add("col-span-full", "1 / -1");
        return dict.ToImmutableDictionary();
    }
}