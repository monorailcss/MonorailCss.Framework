using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the position of bullets/numbers in lists.
/// </summary>
internal class ListStylePositionUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "list-inside", ("list-style-position", "inside") },
            { "list-outside", ("list-style-position", "outside") },
        }.ToImmutableDictionary();
}