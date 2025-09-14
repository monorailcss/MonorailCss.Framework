using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Tables;

/// <summary>
/// Handles caption side utilities (caption-top, caption-bottom).
/// Maps to the CSS caption-side property for table caption elements.
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