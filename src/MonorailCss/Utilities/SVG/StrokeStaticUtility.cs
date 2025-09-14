using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.SVG;

/// <summary>
/// Handles static SVG stroke utilities (stroke-none, stroke-current).
/// For color and width-based stroke utilities, see StrokeUtility.
/// </summary>
internal class StrokeStaticUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "stroke-none", ("stroke", "none") },
            { "stroke-current", ("stroke", "currentcolor") },
        }.ToImmutableDictionary();
}