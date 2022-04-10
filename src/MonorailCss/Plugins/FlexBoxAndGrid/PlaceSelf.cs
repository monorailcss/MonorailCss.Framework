using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The place-self plugin.
/// </summary>
public class PlaceSelf : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "place-self";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities => new Dictionary<string, string>
    {
        { "place-self-auto", "auto" },
        { "place-self-start", "start" },
        { "place-self-end", "end" },
        { "place-self-center", "center" },
        { "place-self-stretch", "stretch" },
    }.ToImmutableDictionary();
}