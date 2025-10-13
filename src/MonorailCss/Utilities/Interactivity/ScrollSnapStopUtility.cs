using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling whether you can skip past possible snap positions.
/// </summary>
internal class ScrollSnapStopUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        new Dictionary<string, (string, string)>
        {
            ["snap-normal"] = ("scroll-snap-stop", "normal"),
            ["snap-always"] = ("scroll-snap-stop", "always"),
        }.ToImmutableDictionary();
}