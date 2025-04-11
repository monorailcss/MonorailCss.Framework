using System.Collections.Immutable;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The font-stretch plugin.
/// </summary>
public class FontStretch : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "font-stretch";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "font-stretch-ultra-condensed", "ultra-condensed" },
            { "font-stretch-extra-condensed", "extra-condensed" },
            { "font-stretch-condensed", "condensed" },
            { "font-stretch-semi-condensed", "semi-condensed" },
            { "font-stretch-normal", "normal" },
            { "font-stretch-semi-expanded", "semi-expanded" },
            { "font-stretch-expanded", "expanded" },
            { "font-stretch-extra-expanded", "extra-expanded" },
            { "font-stretch-ultra-expanded", "ultra-expanded" },
        }.ToImmutableDictionary();
}