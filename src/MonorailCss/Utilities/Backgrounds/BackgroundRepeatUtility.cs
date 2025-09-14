using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Handles background repeat utilities (bg-repeat, bg-no-repeat, bg-repeat-x, etc.).
/// Maps to the CSS background-repeat property.
/// </summary>
internal class BackgroundRepeatUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "bg-repeat", ("background-repeat", "repeat") },
            { "bg-no-repeat", ("background-repeat", "no-repeat") },
            { "bg-repeat-x", ("background-repeat", "repeat-x") },
            { "bg-repeat-y", ("background-repeat", "repeat-y") },
            { "bg-repeat-round", ("background-repeat", "round") },
            { "bg-repeat-space", ("background-repeat", "space") },
        }.ToImmutableDictionary();
}