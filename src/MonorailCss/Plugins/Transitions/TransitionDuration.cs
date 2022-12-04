using System.Collections.Immutable;

namespace MonorailCss.Plugins.Transitions;

/// <summary>
/// The transition duration plugin.
/// </summary>
public class TransitionDuration : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "transition-duration";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "duration-75", "75ms" },
            { "duration-100", "100ms" },
            { "duration-150", "150ms" },
            { "duration-200", "200ms" },
            { "duration-300", "300ms" },
            { "duration-500", "500ms" },
            { "duration-700", "700ms" },
            { "duration-1000", "1000ms" },
        }.ToImmutableDictionary();
}