using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utilities for controlling the clipping area of background images and colors.
/// </summary>
internal class BackgroundClipUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "bg-clip-text", ("background-clip", "text") },
            { "bg-clip-border", ("background-clip", "border-box") },
            { "bg-clip-padding", ("background-clip", "padding-box") },
            { "bg-clip-content", ("background-clip", "content-box") },
        }.ToImmutableDictionary();
}