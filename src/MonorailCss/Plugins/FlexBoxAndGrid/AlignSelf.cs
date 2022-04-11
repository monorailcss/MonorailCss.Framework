using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The align-self plugin.
/// </summary>
public class AlignSelf : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "align-self";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "self-auto", "auto" },
            { "self-start", "flex-start" },
            { "self-end", "flex-end" },
            { "self-center", "center" },
            { "self-stretch", "stretch" },
            { "self-baseline", "baseline" },
        }.ToImmutableDictionary();
}