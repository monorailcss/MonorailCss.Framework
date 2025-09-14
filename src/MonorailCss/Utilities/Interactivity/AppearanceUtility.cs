using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Handles appearance utilities (appearance-none, appearance-auto).
/// </summary>
internal class AppearanceUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "appearance-none", ("appearance", "none") },
            { "appearance-auto", ("appearance", "auto") },
        }.ToImmutableDictionary();
}