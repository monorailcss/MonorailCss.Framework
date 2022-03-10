using System.Collections.Immutable;

namespace MonorailCss.Plugins;

/// <summary>
/// The float plugin.
/// </summary>
public class Float : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "float";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities => new Dictionary<string, string>
    {
        { "float-left", "left" },
        { "float-right", "right" },
        { "float-none", "none" },
    }.ToImmutableDictionary();
}