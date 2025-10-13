using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling whether an element responds to pointer events.
/// </summary>
internal class PointerEventsUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "pointer-events-auto", ("pointer-events", "auto") },
            { "pointer-events-none", ("pointer-events", "none") },
        }.ToImmutableDictionary();
}