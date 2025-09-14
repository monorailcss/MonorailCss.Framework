using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Handles box-sizing utilities (box-border, box-content).
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