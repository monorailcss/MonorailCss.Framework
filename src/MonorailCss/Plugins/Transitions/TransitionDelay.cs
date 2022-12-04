using System.Collections.Immutable;

namespace MonorailCss.Plugins.Transitions;

/// <summary>
/// The transition delay plugin.
/// </summary>
public class TransitionDelay : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "transition-delay";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "delay-75", "75ms" },
            { "delay-100", "100ms" },
            { "delay-150", "150ms" },
            { "delay-200", "200ms" },
            { "delay-300", "300ms" },
            { "delay-500", "500ms" },
            { "delay-700", "700ms" },
            { "delay-1000", "1000ms" },
        }.ToImmutableDictionary();
}