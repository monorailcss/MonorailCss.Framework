using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Handles clear utilities (clear-left, clear-right, etc.).
/// </summary>
internal class ClearUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "clear-start", ("clear", "inline-start") },
            { "clear-end", ("clear", "inline-end") },
            { "clear-left", ("clear", "left") },
            { "clear-right", ("clear", "right") },
            { "clear-both", ("clear", "both") },
            { "clear-none", ("clear", "none") },
        }.ToImmutableDictionary();
}