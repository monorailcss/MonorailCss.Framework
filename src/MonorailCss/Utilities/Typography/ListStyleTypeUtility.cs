using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles list style type utilities (list-none, list-disc, list-decimal).
/// Maps to the CSS list-style-type property for list elements.
/// </summary>
internal class ListStyleTypeUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "list-none", ("list-style-type", "none") },
            { "list-disc", ("list-style-type", "disc") },
            { "list-decimal", ("list-style-type", "decimal") },
        }.ToImmutableDictionary();
}