using System.Collections.Immutable;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The font-style plugin.
/// </summary>
public class FontStyle : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "font-style";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "italic", "italic" },
            { "not-italic", "normal" },
        }.ToImmutableDictionary();
}