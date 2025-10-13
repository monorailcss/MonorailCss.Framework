using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Backgrounds;

/// <summary>
/// Utilities for controlling how background images repeat.
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