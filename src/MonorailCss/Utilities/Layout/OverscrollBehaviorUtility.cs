using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Handles overscroll behavior utilities (overscroll-auto, overscroll-contain, overscroll-none, overscroll-x-*, overscroll-y-*).
/// </summary>
internal class OverscrollBehaviorUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            // General overscroll behavior
            { "overscroll-auto", ("overscroll-behavior", "auto") },
            { "overscroll-contain", ("overscroll-behavior", "contain") },
            { "overscroll-none", ("overscroll-behavior", "none") },

            // X-axis overscroll behavior
            { "overscroll-x-auto", ("overscroll-behavior-x", "auto") },
            { "overscroll-x-contain", ("overscroll-behavior-x", "contain") },
            { "overscroll-x-none", ("overscroll-behavior-x", "none") },

            // Y-axis overscroll behavior
            { "overscroll-y-auto", ("overscroll-behavior-y", "auto") },
            { "overscroll-y-contain", ("overscroll-behavior-y", "contain") },
            { "overscroll-y-none", ("overscroll-behavior-y", "none") },
        }.ToImmutableDictionary();
}