using System.Collections.Immutable;
using MonorailCss.Utilities.Base;

namespace MonorailCss.Utilities.Transforms;

/// <summary>
/// Utilities for controlling the transformation of an element.
/// </summary>
internal class TransformUtility : BaseStaticUtility
{
    protected override ImmutableDictionary<string, (string Property, string Value)> StaticValues =>
        ImmutableDictionary.CreateRange(new Dictionary<string, (string, string)>
        {
            ["transform"] = ("transform", "var(--tw-rotate-x) var(--tw-rotate-y) var(--tw-rotate-z) var(--tw-skew-x) var(--tw-skew-y)"),
            ["transform-cpu"] = ("transform", "var(--tw-rotate-x) var(--tw-rotate-y) var(--tw-rotate-z) var(--tw-skew-x) var(--tw-skew-y)"),
            ["transform-gpu"] = ("transform", "translateZ(0) var(--tw-rotate-x) var(--tw-rotate-y) var(--tw-rotate-z) var(--tw-skew-x) var(--tw-skew-y)"),
            ["transform-none"] = ("transform", "none"),
        });
}