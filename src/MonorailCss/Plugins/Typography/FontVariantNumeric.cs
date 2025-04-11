using System.Collections.Immutable;

namespace MonorailCss.Plugins.Typography;

/// <summary>
/// The font-variant plugin.
/// </summary>
public class FontVariantNumeric : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "font-variant";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> GetUtilities() =>
        new Dictionary<string, string>
        {
            { "normal-nums", "normal" },
            { "ordinal", "ordinal" },
            { "slashed-zero", "slashed-zero" },
            { "lining-nums", "lining-nums" },
            { "oldstyle-nums", "oldstyle-nums" },
            { "proportional-nums", "proportional-nums" },
            { "tabular-nums", "tabular-nums" },
            { "diagonal-fractions", "diagonal-fractions" },
            { "stacked-fractions", "stacked-fractions" },
        }.ToImmutableDictionary();
}