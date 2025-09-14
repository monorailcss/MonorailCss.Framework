using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Typography;

/// <summary>
/// Handles text-decoration-style utilities (decoration-solid, decoration-dashed, etc.).
/// </summary>
internal class TextDecorationStyleUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "decoration-solid", ("text-decoration-style", "solid") },
            { "decoration-double", ("text-decoration-style", "double") },
            { "decoration-dotted", ("text-decoration-style", "dotted") },
            { "decoration-dashed", ("text-decoration-style", "dashed") },
            { "decoration-wavy", ("text-decoration-style", "wavy") },
        }.ToImmutableDictionary();
}