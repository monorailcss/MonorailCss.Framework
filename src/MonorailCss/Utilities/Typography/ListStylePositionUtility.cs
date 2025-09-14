using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles list style position utilities (list-inside, list-outside).
/// Maps to the CSS list-style-position property for list elements.
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