using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utility for transform-box values.
/// Handles: transform-content, transform-border, transform-fill, transform-stroke, transform-view
/// CSS: transform-box property with content-box, border-box, fill-box, stroke-box, and view-box values.
/// </summary>
internal class TransformBoxUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        new Dictionary<string, (string, string)>
        {
            ["transform-content"] = ("transform-box", "content-box"),
            ["transform-border"] = ("transform-box", "border-box"),
            ["transform-fill"] = ("transform-box", "fill-box"),
            ["transform-stroke"] = ("transform-box", "stroke-box"),
            ["transform-view"] = ("transform-box", "view-box"),
        }.ToImmutableDictionary();
}