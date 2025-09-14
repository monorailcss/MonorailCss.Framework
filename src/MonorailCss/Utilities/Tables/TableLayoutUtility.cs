using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Tables;

/// <summary>
/// Handles table layout utilities (table-auto, table-fixed).
/// Maps to the CSS table-layout property for table elements.
/// </summary>
internal class TableLayoutUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "table-auto", ("table-layout", "auto") },
            { "table-fixed", ("table-layout", "fixed") },
        }.ToImmutableDictionary();
}