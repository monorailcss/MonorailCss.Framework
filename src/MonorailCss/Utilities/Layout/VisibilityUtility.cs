using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Handles visibility utilities (visible, invisible, collapse).
/// </summary>
internal class VisibilityUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "visible", ("visibility", "visible") },
            { "invisible", ("visibility", "hidden") },
            { "collapse", ("visibility", "collapse") },
        }.ToImmutableDictionary();
}