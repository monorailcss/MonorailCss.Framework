using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Handles display utilities (block, flex, grid, etc.).
/// </summary>
internal class DisplayUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "block", ("display", "block") },
            { "inline", ("display", "inline") },
            { "inline-block", ("display", "inline-block") },
            { "flex", ("display", "flex") },
            { "inline-flex", ("display", "inline-flex") },
            { "grid", ("display", "grid") },
            { "inline-grid", ("display", "inline-grid") },
            { "table", ("display", "table") },
            { "inline-table", ("display", "inline-table") },
            { "table-caption", ("display", "table-caption") },
            { "table-cell", ("display", "table-cell") },
            { "table-column", ("display", "table-column") },
            { "table-column-group", ("display", "table-column-group") },
            { "table-footer-group", ("display", "table-footer-group") },
            { "table-header-group", ("display", "table-header-group") },
            { "table-row", ("display", "table-row") },
            { "table-row-group", ("display", "table-row-group") },
            { "flow-root", ("display", "flow-root") },
            { "contents", ("display", "contents") },
            { "list-item", ("display", "list-item") },
            { "none", ("display", "none") },
            { "hidden", ("display", "none") },
        }.ToImmutableDictionary();
}