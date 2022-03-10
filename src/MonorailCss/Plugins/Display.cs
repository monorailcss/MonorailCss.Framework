using System.Collections.Immutable;

namespace MonorailCss.Plugins;

/// <summary>
/// The display plugin.
/// </summary>
public class Display : BaseUtilityPlugin
{
    /// <inheritdoc />
    protected override string Property => "display";

    /// <inheritdoc />
    protected override ImmutableDictionary<string, string> Utilities => new Dictionary<string, string>
    {
        { "block", "block" },
        { "inline-block", "inline-block" },
        { "inline", "inline" },
        { "flex", "flex" },
        { "inline-flex", "inline-flex" },
        { "table", "table" },
        { "inline-table", "inline-table" },
        { "table-caption", "table-caption" },
        { "table-cell", "table-cell" },
        { "table-column", "table-column" },
        { "table-column-group", "table-column-group" },
        { "table-footer-group", "table-footer-group" },
        { "table-header-group", "table-header-group" },
        { "table-row-group", "table-row-group" },
        { "table-row", "table-row" },
        { "flow-root", "flow-root" },
        { "grid", "grid" },
        { "inline-grid", "inline-grid" },
        { "contents", "contents" },
        { "list-item", "list-item" },
        { "hidden", "wnone" },
    }.ToImmutableDictionary();
}
