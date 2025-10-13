using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Tables;

/// <summary>
/// Utilities for controlling the table layout algorithm.
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