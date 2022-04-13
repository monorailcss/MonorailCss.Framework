using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The flex-shrink plugin.
/// </summary>
public class FlexShrink : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "flex-shrink";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string> { { "flex-shrink", "1" }, { "flex-shrink-0", "0" }, }.ToImmutableDictionary();
}