using System.Collections.Immutable;

namespace MonorailCss.Plugins.Interactivity;

/// <summary>
/// The scroll-behavior plugin.
/// </summary>
public class ScrollBehavior : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "scroll-behavior";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        return new Dictionary<string, string>
        {
            { "scroll-auto", "auto" },
            { "scroll-smooth", "smooth" },
        }.ToImmutableDictionary();
    }
}