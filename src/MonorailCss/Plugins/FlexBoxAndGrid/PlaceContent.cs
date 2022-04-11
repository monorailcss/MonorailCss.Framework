using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The place-content plugin.
/// </summary>
public class PlaceContent : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "place-content";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "place-content-center", "center" },
            { "place-content-start", "start" },
            { "place-content-end", "end" },
            { "place-content-between", "space-between" },
            { "place-content-around", "space-around" },
            { "place-content-evenly", "space-evenly" },
            { "place-content-stretch", "stretch" },
        }.ToImmutableDictionary();
}