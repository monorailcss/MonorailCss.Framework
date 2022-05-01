using System.Collections.Immutable;

namespace MonorailCss.Plugins.Interactivity;

/// <summary>
/// The appearance plugin.
/// </summary>
public class Appearance : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "appearance";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        return new Dictionary<string, string> { { "appearance-none", "none" }, }.ToImmutableDictionary();
    }
}