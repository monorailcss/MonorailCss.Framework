using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for creating container query names.
/// </summary>
internal class ContainerQueryUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "@container", ("container-type", "inline-size") },
            { "@container-normal", ("container-type", "normal") },
        }.ToImmutableDictionary();
}