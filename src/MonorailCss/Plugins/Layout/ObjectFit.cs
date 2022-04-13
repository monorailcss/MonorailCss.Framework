using System.Collections.Immutable;

namespace MonorailCss.Plugins.Layout;

/// <summary>
/// The object-fit plugin.
/// </summary>
public class ObjectFit : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "object-fit";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        return new Dictionary<string, string>
        {
            { "object-contain", "contain" },
            { "object-cover", "cover" },
            { "object-fill", "fill" },
            { "object-none", "none" },
            { "object-scale", " scale-down" },
        }.ToImmutableDictionary();
    }
}