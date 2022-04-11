using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The justify-content plugin.
/// </summary>
public class JustifyContent : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "justify-content";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>()
        {
            { "justify-start", "flex-start" },
            { "justify-end", "flex-end" },
            { "justify-center", "center" },
            { "justify-between", "space-between" },
            { "justify-around", "space-around" },
            { "justify-evenly", "space-evenly" },
        }.ToImmutableDictionary();
}