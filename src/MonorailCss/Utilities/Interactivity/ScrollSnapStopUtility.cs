using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utility for scroll-snap-stop values.
/// Handles: snap-normal, snap-always
/// CSS: scroll-snap-stop property with normal and always values.
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