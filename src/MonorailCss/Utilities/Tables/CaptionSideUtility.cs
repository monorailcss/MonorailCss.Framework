using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Tables;

/// <summary>
/// Utilities for controlling the placement of a table caption.
/// </summary>
internal class CaptionSideUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "caption-top", ("caption-side", "top") },
            { "caption-bottom", ("caption-side", "bottom") },
        }.ToImmutableDictionary();
}