using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling how items are justified and aligned at the same time.
/// </summary>
internal class PlaceUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            // Place Content utilities
            { "place-content-center", ("place-content", "center") },
            { "place-content-start", ("place-content", "start") },
            { "place-content-end", ("place-content", "end") },
            { "place-content-between", ("place-content", "space-between") },
            { "place-content-around", ("place-content", "space-around") },
            { "place-content-evenly", ("place-content", "space-evenly") },
            { "place-content-baseline", ("place-content", "baseline") },
            { "place-content-stretch", ("place-content", "stretch") },
            { "place-content-center-safe", ("place-content", "safe center") },
            { "place-content-end-safe", ("place-content", "safe end") },

            // Place Items utilities
            { "place-items-start", ("place-items", "start") },
            { "place-items-end", ("place-items", "end") },
            { "place-items-center", ("place-items", "center") },
            { "place-items-baseline", ("place-items", "baseline") },
            { "place-items-stretch", ("place-items", "stretch") },
            { "place-items-center-safe", ("place-items", "safe center") },
            { "place-items-end-safe", ("place-items", "safe end") },

            // Place Self utilities
            { "place-self-auto", ("place-self", "auto") },
            { "place-self-start", ("place-self", "start") },
            { "place-self-end", ("place-self", "end") },
            { "place-self-center", ("place-self", "center") },
            { "place-self-stretch", ("place-self", "stretch") },
            { "place-self-center-safe", ("place-self", "safe center") },
            { "place-self-end-safe", ("place-self", "safe end") },
        }.ToImmutableDictionary();

    public string[] GetDocumentedProperties() => ["place-content", "place-items", "place-self"];
}