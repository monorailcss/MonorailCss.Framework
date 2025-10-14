using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling how flex and grid items are positioned along a container's cross axis.
/// </summary>
internal class AlignItemsUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "items-center", ("align-items", "center") },
            { "items-start", ("align-items", "flex-start") },
            { "items-end", ("align-items", "flex-end") },
            { "items-baseline", ("align-items", "baseline") },
            { "items-stretch", ("align-items", "stretch") },
            { "items-baseline-last", ("align-items", "last baseline") },
            { "items-center-safe", ("align-items", "safe center") },
            { "items-end-safe", ("align-items", "safe flex-end") },
        }.ToImmutableDictionary();

    public string[] GetDocumentedProperties() => ["align-items"];
}