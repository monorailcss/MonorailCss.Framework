using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling how an element handles content that is too large for the container.
/// </summary>
internal class OverflowUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "overflow-auto", ("overflow", "auto") },
            { "overflow-hidden", ("overflow", "hidden") },
            { "overflow-clip", ("overflow", "clip") },
            { "overflow-visible", ("overflow", "visible") },
            { "overflow-scroll", ("overflow", "scroll") },
            { "overflow-x-auto", ("overflow-x", "auto") },
            { "overflow-x-hidden", ("overflow-x", "hidden") },
            { "overflow-x-clip", ("overflow-x", "clip") },
            { "overflow-x-visible", ("overflow-x", "visible") },
            { "overflow-x-scroll", ("overflow-x", "scroll") },
            { "overflow-y-auto", ("overflow-y", "auto") },
            { "overflow-y-hidden", ("overflow-y", "hidden") },
            { "overflow-y-clip", ("overflow-y", "clip") },
            { "overflow-y-visible", ("overflow-y", "visible") },
            { "overflow-y-scroll", ("overflow-y", "scroll") },
            { "overflow-ellipsis", ("text-overflow", "ellipsis") },
        }.ToImmutableDictionary();
}