using System.Collections.Immutable;

namespace MonorailCss.Plugins.Transitions;

/// <summary>
/// The transition duration plugin.
/// </summary>
public class TransitionTiming : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "transition-timing-function";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "ease-linear", "linear" },
            { "ease-in", "cubic-bezier(0.4, 0, 1, 1)" },
            { "ease-out", "cubic-bezier(0, 0, 0.2, 1)" },
            { "ease-in-out", "cubic-bezier(0.4, 0, 0.2, 1)" },
        }.ToImmutableDictionary();
}