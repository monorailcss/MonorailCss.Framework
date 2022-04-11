using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The flex-shrink plugin.
/// </summary>
public class FlexWrap : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "flex-wrap";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>()
        {
            { "flex-wrap", "wrap" },
            { "flex-wrap-reverse", "wrap-reverse" },
            { "flex-nowrap", "nowrap" },
        }.ToImmutableDictionary();
}