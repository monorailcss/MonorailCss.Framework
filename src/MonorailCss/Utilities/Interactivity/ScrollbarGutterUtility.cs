using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for reserving space for the scrollbar gutter.
/// </summary>
internal class ScrollbarGutterUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "scrollbar-gutter-auto", ("scrollbar-gutter", "auto") },
            { "scrollbar-gutter-stable", ("scrollbar-gutter", "stable") },
            { "scrollbar-gutter-both", ("scrollbar-gutter", "stable both-edges") },
        }.ToImmutableDictionary();
}
