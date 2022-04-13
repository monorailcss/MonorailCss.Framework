using System.Collections.Immutable;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The letter-spacing plugin.
/// </summary>
public class LetterSpacing : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "letter-spacing";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        return new Dictionary<string, string>
        {
            { "tracking-tighter", "-0.05em" },
            { "tracking-tight", "-0.025em" },
            { "tracking-normal", "0em" },
            { "tracking-wide", "0.025em" },
            { "tracking-wider", "0.05em" },
            { "tracking-widest", "0.1em" },
        }.ToImmutableDictionary();
    }
}