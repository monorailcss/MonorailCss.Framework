using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling the scroll behavior of an element.
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