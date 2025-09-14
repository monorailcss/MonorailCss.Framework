using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Layout;

/// <summary>
/// Utility for CSS containment properties.
/// Handles: contain-none, contain-strict, contain-content, contain-size, contain-layout, contain-style, contain-paint
/// CSS: contain property values for layout performance optimization.
/// </summary>
internal class ContainUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues { get; } =
        new Dictionary<string, (string, string)>
        {
            { "contain-none", ("contain", "none") },
            { "contain-strict", ("contain", "strict") },
            { "contain-content", ("contain", "content") },
            { "contain-size", ("contain", "size") },
            { "contain-layout", ("contain", "layout") },
            { "contain-style", ("contain", "style") },
            { "contain-paint", ("contain", "paint") },
        }.ToImmutableDictionary();
}