using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Handles grid-flow utilities (grid-flow-row, grid-flow-col, grid-flow-dense, etc.).
/// Maps to the CSS grid-auto-flow property.
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
}