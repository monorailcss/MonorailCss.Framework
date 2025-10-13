using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling the visibility of an element.
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