using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utilities for controlling the box model used for transforms.
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