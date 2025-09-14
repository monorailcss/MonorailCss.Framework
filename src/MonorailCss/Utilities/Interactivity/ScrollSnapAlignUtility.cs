using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utility for scroll-snap-align values.
/// Handles: snap-start, snap-end, snap-center, snap-align-none
/// CSS: scroll-snap-align property with alignment keywords.
/// </summary>
internal class ScrollSnapAlignUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        new Dictionary<string, (string, string)>
        {
            ["snap-start"] = ("scroll-snap-align", "start"),
            ["snap-end"] = ("scroll-snap-align", "end"),
            ["snap-center"] = ("scroll-snap-align", "center"),
            ["snap-align-none"] = ("scroll-snap-align", "none"),
        }.ToImmutableDictionary();
}