using System.Collections.Immutable;

namespace MonorailCss.Plugins.Svg;

/// <summary>
/// The stroke-width plugin.
/// </summary>
public class StrokeWidth : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "stroke-width";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>()
        {
            { "stroke-0", "0" }, { "stroke-1", "1" }, { "stroke-2", "2" },
        }.ToImmutableDictionary();
}