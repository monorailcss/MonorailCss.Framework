using System.Collections.Immutable;

namespace MonorailCss.Plugins.Layout;

/// <summary>
/// The z-index plugin.
/// </summary>
public class ZIndex : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "z-index";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities => new Dictionary<string, string>()
    {
        { "z-0", "0" },
        { "z-10", "10" },
        { "z-20", "20" },
        { "z-30", "30" },
        { "z-40", "40" },
        { "z-50", "50" },
        { "z-auto", "auto" },
    }.ToImmutableDictionary();
}