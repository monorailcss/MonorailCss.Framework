using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Handles background clip utilities (bg-clip-text, bg-clip-border, bg-clip-padding, bg-clip-content).
/// Maps to the CSS background-clip property.
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