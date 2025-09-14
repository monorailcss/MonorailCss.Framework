using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Handles flex-direction utilities (flex-row, flex-col, etc.).
/// </summary>
internal class FlexDirectionUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "flex-row", ("flex-direction", "row") },
            { "flex-row-reverse", ("flex-direction", "row-reverse") },
            { "flex-col", ("flex-direction", "column") },
            { "flex-col-reverse", ("flex-direction", "column-reverse") },
        }.ToImmutableDictionary();
}