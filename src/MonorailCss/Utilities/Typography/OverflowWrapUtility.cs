using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles overflow-wrap utilities.
/// Handles: wrap-normal, wrap-anywhere, wrap-break-word.
/// CSS: overflow-wrap: normal, overflow-wrap: anywhere, overflow-wrap: break-word.
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