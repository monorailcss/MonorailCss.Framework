using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling the order of flex and grid items.
/// </summary>
internal class OrderStaticUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "order-first", ("order", "-9999") },
            { "order-last", ("order", "9999") },
            { "order-none", ("order", "0") },
        }.ToImmutableDictionary();

    public string[] GetDocumentedProperties() => ["order"];
}