using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Utilities for controlling how words should break within an element.
/// </summary>
internal class OverflowWrapUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string Property, string Value)>
        {
            ["wrap-normal"] = ("overflow-wrap", "normal"),
            ["wrap-anywhere"] = ("overflow-wrap", "anywhere"),
            ["wrap-break-word"] = ("overflow-wrap", "break-word"),
        });
}