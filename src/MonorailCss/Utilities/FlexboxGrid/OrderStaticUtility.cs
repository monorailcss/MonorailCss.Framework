using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Static utility for flex order special values.
/// Handles: order-first, order-last, order-none
/// CSS: order: -9999, order: 9999, order: 0.
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
}