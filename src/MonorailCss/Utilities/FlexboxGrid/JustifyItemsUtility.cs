using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.FlexboxGrid;

/// <summary>
/// Utilities for controlling how grid items are aligned along their inline axis.
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
            { "justify-items-normal", ("justify-items", "normal") },
            { "justify-items-center-safe", ("justify-items", "safe center") },
            { "justify-items-end-safe", ("justify-items", "safe end") },
        }.ToImmutableDictionary();
}