using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Handles background attachment utilities (bg-fixed, bg-local, bg-scroll).
/// Maps to the CSS background-attachment property.
/// </summary>
internal class BackgroundAttachmentUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "bg-fixed", ("background-attachment", "fixed") },
            { "bg-local", ("background-attachment", "local") },
            { "bg-scroll", ("background-attachment", "scroll") },
        }.ToImmutableDictionary();
}