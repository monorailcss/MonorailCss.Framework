using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Handles float utilities (float-left, float-right, etc.).
/// </summary>
internal class FloatUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "float-start", ("float", "inline-start") },
            { "float-end", ("float", "inline-end") },
            { "float-left", ("float", "left") },
            { "float-right", ("float", "right") },
            { "float-none", ("float", "none") },
        }.ToImmutableDictionary();
}