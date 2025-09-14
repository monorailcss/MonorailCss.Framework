using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Tables;

/// <summary>
/// Handles border collapse utilities (border-collapse, border-separate).
/// Maps to the CSS border-collapse property for table elements.
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