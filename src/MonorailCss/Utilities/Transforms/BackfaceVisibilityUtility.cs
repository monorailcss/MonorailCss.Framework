using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utilities for controlling the visibility of an element's back face when rotated.
/// </summary>
internal class BackfaceVisibilityUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        new Dictionary<string, (string, string)>
        {
            ["backface-visible"] = ("backface-visibility", "visible"),
            ["backface-hidden"] = ("backface-visibility", "hidden"),
        }.ToImmutableDictionary();
}