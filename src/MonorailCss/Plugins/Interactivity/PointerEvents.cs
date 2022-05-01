using System.Collections.Immutable;

namespace MonorailCss.Plugins.Interactivity;

/// <summary>
/// The pointer-events plugin.
/// </summary>
public class PointerEvents : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "pointer-events";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        return new Dictionary<string, string>
        {
            { "pointer-events-none", "none" }, { "pointer-events-auto", "auto" },
        }.ToImmutableDictionary();
    }
}