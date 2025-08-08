using System.Collections.Immutable;

namespace MonorailCss.Plugins.Interactivity;

/// <summary>
/// The scrollbar-width plugin.
/// </summary>
public class ScrollbarWidth : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "scrollbar-width";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        return new Dictionary<string, string>
        {
            { "scrollbar-thin", "thin" },
            { "scrollbar-auto", "auto" },
            { "scrollbar-none", "none" },
        }.ToImmutableDictionary();
    }
}