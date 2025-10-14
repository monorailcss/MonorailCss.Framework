using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling how elements in a grid are auto-placed.
/// </summary>
internal class GridFlowUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "grid-flow-row", ("grid-auto-flow", "row") },
            { "grid-flow-col", ("grid-auto-flow", "column") },
            { "grid-flow-dense", ("grid-auto-flow", "dense") },
            { "grid-flow-row-dense", ("grid-auto-flow", "row dense") },
            { "grid-flow-col-dense", ("grid-auto-flow", "column dense") },
        }.ToImmutableDictionary();

    public string[]? GetDocumentedProperties() => ["grid-auto-flow"];
}