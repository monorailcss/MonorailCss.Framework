using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Handles resize utilities (resize-none, resize, resize-x, resize-y).
/// </summary>
internal class ResizeUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "resize-none", ("resize", "none") },
            { "resize", ("resize", "both") },
            { "resize-x", ("resize", "horizontal") },
            { "resize-y", ("resize", "vertical") },
        }.ToImmutableDictionary();
}