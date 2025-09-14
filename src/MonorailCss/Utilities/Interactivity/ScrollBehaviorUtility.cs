using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Handles scroll behavior utilities (scroll-auto, scroll-smooth).
/// Maps to the CSS scroll-behavior property for smooth scrolling.
/// </summary>
internal class ScrollBehaviorUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "scroll-auto", ("scroll-behavior", "auto") },
            { "scroll-smooth", ("scroll-behavior", "smooth") },
        }.ToImmutableDictionary();
}