using System.Collections.Immutable;

namespace MonorailCss.Plugins.Spacing;

/// <summary>
/// The align-items plugin.
/// </summary>
public class AlignItems : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "align-items";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "items-start", "flex-start" },
            { "items-end", "flex-end" },
            { "items-center", "center" },
            { "items-baseline", "baseline" },
            { "items-stretch", "stretch" },
        }.ToImmutableDictionary();
}