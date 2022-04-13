using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The grid-template-columns plugin.
/// </summary>
public class GridRowSpan : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "grid-row";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        var dict = new Dictionary<string, string>();

        for (var i = 1; i <= 6; i++)
        {
            dict.Add($"row-span-{i}", $"span {i} / span {i}");
        }

        dict.Add("row-auto", "auto");
        dict.Add("row-span-full", "1 / -1");
        return dict.ToImmutableDictionary();
    }
}