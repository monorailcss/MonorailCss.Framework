using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utility for static perspective values.
/// Handles: perspective-none
/// CSS: perspective property with none keyword.
/// </summary>
internal class PerspectiveStaticUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        new Dictionary<string, (string, string)>
        {
            ["perspective-none"] = ("perspective", "none"),
        }.ToImmutableDictionary();
}