using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Static utility for z-index auto value.
/// Handles: z-auto
/// CSS: z-index: auto.
/// </summary>
internal class ZIndexStaticUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "z-auto", ("z-index", "auto") },
        }.ToImmutableDictionary();
}