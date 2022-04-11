using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The align-content plugin.
/// </summary>
public class AlignContent : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "align-content";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "content-center", "center" },
            { "content-start", "flex-start" },
            { "content-end", "flex-end" },
            { "content-between", "space-between" },
            { "content-around", "space-around" },
            { "content-evenly", "space-evenly" },
        }.ToImmutableDictionary();
}