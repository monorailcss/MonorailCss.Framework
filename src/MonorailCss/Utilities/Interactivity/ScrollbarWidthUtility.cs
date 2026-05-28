using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling the width of an element's scrollbars.
/// </summary>
internal class ScrollbarWidthUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "scrollbar-auto", ("scrollbar-width", "auto") },
            { "scrollbar-thin", ("scrollbar-width", "thin") },
            { "scrollbar-none", ("scrollbar-width", "none") },
        }.ToImmutableDictionary();
}
