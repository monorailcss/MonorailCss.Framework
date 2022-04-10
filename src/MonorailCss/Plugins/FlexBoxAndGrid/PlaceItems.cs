using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The place-items plugin.
/// </summary>
public class PlaceItems : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "place-items";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities => new Dictionary<string, string>
    {
        { "place-items-start", "start" },
        { "place-items-end", "end" },
        { "place-items-center", "center" },
        { "place-items-stretch", "stretch" },
    }.ToImmutableDictionary();
}