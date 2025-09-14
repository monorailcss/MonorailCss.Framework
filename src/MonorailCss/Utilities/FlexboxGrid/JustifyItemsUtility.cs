using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Handles justify items utilities (justify-items-start, justify-items-end, justify-items-center, justify-items-stretch).
/// </summary>
internal class JustifyItemsUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "justify-items-start", ("justify-items", "start") },
            { "justify-items-end", ("justify-items", "end") },
            { "justify-items-center", ("justify-items", "center") },
            { "justify-items-stretch", ("justify-items", "stretch") },
        }.ToImmutableDictionary();
}