using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The flex plugin.
/// </summary>
public class Flex : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "flex";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities => new Dictionary<string, string>
    {
        { "flex-1", "1 1 0;" }, { "flex-auto", "1 1 auto" }, { "flex-initial", "0 1 auto" }, { "flex-none", "none" },
    }.ToImmutableDictionary();
}