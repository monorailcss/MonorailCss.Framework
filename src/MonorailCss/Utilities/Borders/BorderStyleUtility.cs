using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Borders;

/// <summary>
/// Handles border style utilities (border-solid, border-dashed, border-dotted, etc.).
/// Maps to the CSS border-style property.
/// </summary>
internal class BorderStyleUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "border-solid", ("border-style", "solid") },
            { "border-dashed", ("border-style", "dashed") },
            { "border-dotted", ("border-style", "dotted") },
            { "border-double", ("border-style", "double") },
            { "border-hidden", ("border-style", "hidden") },
            { "border-none", ("border-style", "none") },
        }.ToImmutableDictionary();
}