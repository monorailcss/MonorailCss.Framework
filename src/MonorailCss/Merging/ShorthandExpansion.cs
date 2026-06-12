using System.Collections.Immutable;

namespace MonorailCss.Merging;

/// <summary>
/// Expands a declared CSS property into the full set of conflict keys it overrides, so the
/// superset rule in <see cref="ClassMerger"/> sees that a shorthand beats its longhands:
/// <c>p-4</c> (padding) covers <c>pt-2</c> (padding-top), but not the other way around.
/// Property names mirror exactly what the utilities emit (e.g. <c>px-*</c> emits
/// <c>padding-inline</c>, <c>pl-*</c> emits <c>padding-left</c>).
/// </summary>
/// <remarks>
/// Intentional deviation from tailwind-merge: axis shorthands also cover the logical sides
/// (<c>px-*</c> removes an earlier <c>ps-*</c>) because <c>padding-inline</c> genuinely resets
/// <c>padding-inline-start</c> in CSS; tailwind-merge only lists the physical sides. Shorthands
/// never emitted by any utility (<c>font</c>, <c>background</c>, <c>animation</c>,
/// <c>transition</c>) are deliberately absent — they can only appear via arbitrary properties.
/// </remarks>
internal static class ShorthandExpansion
{
    private static readonly ImmutableDictionary<string, ImmutableArray<string>> _expansions = BuildTable();

    /// <summary>
    /// Returns the conflict keys for a declared property: the property itself plus every
    /// longhand it overrides.
    /// </summary>
    /// <param name="property">The declared property name.</param>
    /// <returns>The property and all keys it covers.</returns>
    public static IEnumerable<string> Expand(string property)
    {
        yield return property;

        if (_expansions.TryGetValue(property, out var extras))
        {
            foreach (var extra in extras)
            {
                yield return extra;
            }
        }
    }

    private static ImmutableDictionary<string, ImmutableArray<string>> BuildTable()
    {
        var table = new Dictionary<string, ImmutableArray<string>>();

        // Box-side families sharing the {base, -inline, -block, physical sides, logical sides} shape.
        AddSideFamily(table, "padding");
        AddSideFamily(table, "margin");
        AddSideFamily(table, "scroll-margin");
        AddSideFamily(table, "scroll-padding");

        // Inset uses bare physical side names (top/right/bottom/left).
        table["inset"] =
        [
            "top", "right", "bottom", "left",
            "inset-inline", "inset-block",
            "inset-inline-start", "inset-inline-end", "inset-block-start", "inset-block-end",
        ];
        table["inset-inline"] = ["left", "right", "inset-inline-start", "inset-inline-end"];
        table["inset-block"] = ["top", "bottom", "inset-block-start", "inset-block-end"];

        // Border width/style/color place the side segment between "border" and the suffix.
        AddBorderFamily(table, "width");
        AddBorderFamily(table, "style");
        AddBorderFamily(table, "color");

        table["border-radius"] =
        [
            "border-top-left-radius", "border-top-right-radius",
            "border-bottom-right-radius", "border-bottom-left-radius",
            "border-start-start-radius", "border-start-end-radius",
            "border-end-end-radius", "border-end-start-radius",
        ];

        table["gap"] = ["column-gap", "row-gap"];
        table["overflow"] = ["overflow-x", "overflow-y"];
        table["overscroll-behavior"] = ["overscroll-behavior-x", "overscroll-behavior-y"];
        table["grid-column"] = ["grid-column-start", "grid-column-end"];
        table["grid-row"] = ["grid-row-start", "grid-row-end"];
        table["place-items"] = ["align-items", "justify-items"];
        table["place-content"] = ["align-content", "justify-content"];
        table["place-self"] = ["align-self", "justify-self"];
        table["flex"] = ["flex-grow", "flex-shrink", "flex-basis"];

        return table.ToImmutableDictionary();
    }

    private static void AddSideFamily(Dictionary<string, ImmutableArray<string>> table, string baseName)
    {
        table[baseName] =
        [
            $"{baseName}-inline", $"{baseName}-block",
            $"{baseName}-top", $"{baseName}-right", $"{baseName}-bottom", $"{baseName}-left",
            $"{baseName}-inline-start", $"{baseName}-inline-end",
            $"{baseName}-block-start", $"{baseName}-block-end",
        ];
        table[$"{baseName}-inline"] =
            [$"{baseName}-left", $"{baseName}-right", $"{baseName}-inline-start", $"{baseName}-inline-end"];
        table[$"{baseName}-block"] =
            [$"{baseName}-top", $"{baseName}-bottom", $"{baseName}-block-start", $"{baseName}-block-end"];
    }

    private static void AddBorderFamily(Dictionary<string, ImmutableArray<string>> table, string suffix)
    {
        table[$"border-{suffix}"] =
        [
            $"border-inline-{suffix}", $"border-block-{suffix}",
            $"border-top-{suffix}", $"border-right-{suffix}", $"border-bottom-{suffix}", $"border-left-{suffix}",
            $"border-inline-start-{suffix}", $"border-inline-end-{suffix}",
            $"border-block-start-{suffix}", $"border-block-end-{suffix}",
        ];
        table[$"border-inline-{suffix}"] =
        [
            $"border-left-{suffix}", $"border-right-{suffix}",
            $"border-inline-start-{suffix}", $"border-inline-end-{suffix}",
        ];
        table[$"border-block-{suffix}"] =
        [
            $"border-top-{suffix}", $"border-bottom-{suffix}",
            $"border-block-start-{suffix}", $"border-block-end-{suffix}",
        ];
    }
}
