using System.Collections.Immutable;

namespace MonorailCss.Plugins.FlexBoxAndGrid;

/// <summary>
/// The flex-grow plugin.
/// </summary>
public class FlexGrow : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "flex-grow";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities =>
        new Dictionary<string, string>() { { "grow", "1" }, { "grow-0", "0" }, }.ToImmutableDictionary();
}