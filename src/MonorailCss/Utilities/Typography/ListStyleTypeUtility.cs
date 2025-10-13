using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling the bullet/number style of a list.
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