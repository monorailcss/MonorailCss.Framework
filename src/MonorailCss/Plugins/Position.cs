using System.Collections.Immutable;

namespace MonorailCss.Plugins;

/// <summary>
/// The position plugin.
/// </summary>
public class Position : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "position";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>()
        {
            { "static", "static" },
            { "fixed", "fixed" },
            { "absolute", "absolute" },
            { "relative", "relative" },
            { "sticky", "sticky" },
        }.ToImmutableDictionary();
}