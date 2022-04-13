using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The flex-grow plugin.
/// </summary>
public class FlexGrow : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "flex-grow";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string> { { "flex-grow", "1" }, { "flex-grow-0", "0" }, }.ToImmutableDictionary();
}