using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utility for CSS container query properties.
/// Handles: @container, @container-normal
/// CSS: container-type: inline-size for basic container query support.
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