using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling how flex and grid items are positioned along a container's main axis.
/// </summary>
internal class JustifyContentUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "justify-normal", ("justify-content", "normal") },
            { "justify-center", ("justify-content", "center") },
            { "justify-start", ("justify-content", "flex-start") },
            { "justify-end", ("justify-content", "flex-end") },
            { "justify-between", ("justify-content", "space-between") },
            { "justify-around", ("justify-content", "space-around") },
            { "justify-evenly", ("justify-content", "space-evenly") },
            { "justify-stretch", ("justify-content", "stretch") },
            { "justify-baseline", ("justify-content", "baseline") },
            { "justify-center-safe", ("justify-content", "safe center") },
            { "justify-end-safe", ("justify-content", "safe flex-end") },
        }.ToImmutableDictionary();

    public string[] GetDocumentedProperties() => ["justify-content"];
}