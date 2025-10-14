using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling how rows are positioned in multi-row flex and grid containers.
/// </summary>
internal class AlignContentUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "content-normal", ("align-content", "normal") },
            { "content-center", ("align-content", "center") },
            { "content-start", ("align-content", "flex-start") },
            { "content-end", ("align-content", "flex-end") },
            { "content-between", ("align-content", "space-between") },
            { "content-around", ("align-content", "space-around") },
            { "content-evenly", ("align-content", "space-evenly") },
            { "content-baseline", ("align-content", "baseline") },
            { "content-stretch", ("align-content", "stretch") },
            { "content-center-safe", ("align-content", "safe center") },
            { "content-end-safe", ("align-content", "safe flex-end") },
        }.ToImmutableDictionary();

    public string[] GetDocumentedProperties() => ["align-content"];
}