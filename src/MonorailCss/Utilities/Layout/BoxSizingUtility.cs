using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utilities for controlling how the browser calculates an element's total size.
/// </summary>
internal class BoxSizingUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "box-border", ("box-sizing", "border-box") },
            { "box-content", ("box-sizing", "content-box") },
        }.ToImmutableDictionary();
}