using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utility for CSS container query properties.
/// Handles: @container
/// CSS: container-type: inline-size for basic container query support.
/// </summary>
internal class ContainerQueryUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "@container", ("container-type", "inline-size") },
        }.ToImmutableDictionary();
}