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

    public override IEnumerable<Documentation.UtilityExample> GetExamples(Theme.Theme theme)
    {
        return new[]
        {
            new Documentation.UtilityExample("block", "Display as a block element", "display: block"),
            new Documentation.UtilityExample("inline", "Display as an inline element", "display: inline"),
            new Documentation.UtilityExample("flex", "Display as a flex container", "display: flex"),
            new Documentation.UtilityExample("inline-flex", "Display as an inline flex container", "display: inline-flex"),
            new Documentation.UtilityExample("grid", "Display as a grid container", "display: grid"),
            new Documentation.UtilityExample("inline-grid", "Display as an inline grid container", "display: inline-grid"),
            new Documentation.UtilityExample("table", "Display as a table element", "display: table"),
            new Documentation.UtilityExample("hidden", "Hide the element", "display: none"),
            new Documentation.UtilityExample("contents", "Display only contents (remove wrapper)", "display: contents"),
            new Documentation.UtilityExample("flow-root", "Create a new block formatting context", "display: flow-root"),
        };
    }

    public Documentation.UtilityMetadata GetMetadata()
    {
        return new Documentation.UtilityMetadata(
            "DisplayUtility",
            "Layout",
            "Controls the display type of an element",
            supportsModifiers: false,
            supportsArbitraryValues: false);
    }
}