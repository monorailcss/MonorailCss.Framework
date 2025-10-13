using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Interactivity;

/// <summary>
/// Utilities for suppressing native form control styling.
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