using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utility for backface-visibility values.
/// Handles: backface-visible, backface-hidden
/// CSS: backface-visibility property with visible and hidden values.
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