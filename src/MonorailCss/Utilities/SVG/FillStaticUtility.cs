using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.SVG;

/// <summary>
/// Handles static SVG fill utilities (fill-none, fill-current).
/// For color-based fill utilities, see FillUtility.
/// </summary>
internal class FillStaticUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "fill-none", ("fill", "none") },
            { "fill-current", ("fill", "currentcolor") },
        }.ToImmutableDictionary();
}