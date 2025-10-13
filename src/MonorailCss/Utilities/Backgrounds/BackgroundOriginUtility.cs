using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utilities for controlling the origin position of background images.
/// </summary>
internal class BackgroundOriginUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "bg-origin-border", ("background-origin", "border-box") },
            { "bg-origin-padding", ("background-origin", "padding-box") },
            { "bg-origin-content", ("background-origin", "content-box") },
        }.ToImmutableDictionary();
}