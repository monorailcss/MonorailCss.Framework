using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The flex-direction plugin.
/// </summary>
public class FlexDirection : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "flex-direction";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities =>
        new Dictionary<string, string>()
        {
            { "flex-row", "row" },
            { "flex-row-reverse", "row-reverse" },
            { "flex-col", "column" },
            { "flex-col-reverse", "column-reverse" },
        }.ToImmutableDictionary();
}