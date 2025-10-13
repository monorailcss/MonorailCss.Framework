using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for controlling how an element can be resized.
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