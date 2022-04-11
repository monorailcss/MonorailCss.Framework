using System.Collections.Immutable;

namespace MonorailCss.Plugins.Effects;

/// <summary>
/// The opacity plugin.
/// </summary>
public class Opacity : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "opacity";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>()
        {
            { "opacity-0", "0" },
            { "opacity-5", "0.05" },
            { "opacity-10", "0.1" },
            { "opacity-20", "0.2" },
            { "opacity-25", "0.25" },
            { "opacity-30", "0.3" },
            { "opacity-40", "0.4" },
            { "opacity-50", "0.5" },
            { "opacity-60", "0.6" },
            { "opacity-70", "0.7" },
            { "opacity-75", "0.75" },
            { "opacity-80", "0.8" },
            { "opacity-90", "0.9" },
            { "opacity-95", "0.95" },
            { "opacity-100", "1" },
        }.ToImmutableDictionary();
}