using System.Collections.Immutable;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The text-align plugin.
/// </summary>
public class TextAlign : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "text-align";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities()
    {
        return new Dictionary<string, string>()
        {
            { "left", "left" }, { "center", "center" }, { "right", "right" }, { "justify", "justify" },
        }.ToImmutableDictionary();
    }
}