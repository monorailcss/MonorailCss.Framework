using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling the scroll snap alignment of an element.
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