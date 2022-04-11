using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The justify-self plugin.
/// </summary>
public class JustifySelf : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "justify-self";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>()
        {
            { "justify-self-auto", "auto" },
            { "justify-self-start", "start" },
            { "justify-self-end", "end" },
            { "justify-self-center", "center" },
            { "justify-self-stretch", "stretch" },
        }.ToImmutableDictionary();
}