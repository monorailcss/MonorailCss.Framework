using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The justify-items plugin.
/// </summary>
public class JustifyItems : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "justify-items";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>()
        {
            { "justify-items-start", "start" },
            { "justify-items-end", "end" },
            { "justify-items-center", "center" },
            { "justify-items-stretch", "stretch" },
        }.ToImmutableDictionary();
}