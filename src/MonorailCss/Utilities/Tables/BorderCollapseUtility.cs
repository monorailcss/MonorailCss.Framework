using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Tables;

/// <summary>
/// Utilities for controlling whether table borders should collapse or be separated.
/// </summary>
internal class BorderCollapseUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "border-collapse", ("border-collapse", "collapse") },
            { "border-separate", ("border-collapse", "separate") },
        }.ToImmutableDictionary();
}