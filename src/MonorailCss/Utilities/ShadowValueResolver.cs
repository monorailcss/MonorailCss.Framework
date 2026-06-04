using MonorailCss.Candidates;
using MonorailCss.DataTypes;

namespace MonorailCss.Utilities;

/// <summary>
/// Shared discrimination for arbitrary inset-shadow / drop-shadow values.
/// </summary>
/// <remarks>
/// The size utilities (inset-shadow / drop-shadow) and the matching color utilities both claim the
/// same functional root, so they must agree on what counts as a <em>shadow value</em> versus a
/// <em>color</em>. A bare single color token (<c>[#0088cc]</c>), an explicit <c>color</c> type hint,
/// or a parens-shorthand color (<c>(color:--c)</c>) is a color; everything else with shadow-like
/// shape (multi-token, <c>inset …</c>, <c>calc(…)</c>, or a hint-less parens shorthand like
/// <c>(--my-shadow)</c>) is a value. Keeping the two predicates complementary makes the dispatch
/// order-independent.
/// </remarks>
internal static class ShadowValueResolver
{
    /// <summary>
    /// True when an arbitrary candidate value should be treated as a shadow value (not a color).
    /// </summary>
    public static bool IsArbitraryShadowValue(CandidateValue value)
    {
        if (value.Kind != ValueKind.Arbitrary)
        {
            return false;
        }

        if (value.DataTypeHint == "color")
        {
            return false;
        }

        // `(--my-shadow)` with no color hint is a shadow value (e.g. `drop-shadow(var(--my-shadow))`).
        if (value.IsParenthesesShorthand)
        {
            return true;
        }

        // The `inset` keyword only appears in shadow values, never colors.
        if (value.Value.StartsWith("inset", StringComparison.Ordinal))
        {
            return true;
        }

        // Otherwise it's a shadow value unless the whole token is a color (#hex, a named color, or a
        // color function like rgb()/color-mix()) — those belong to the color utility. Inference
        // correctly keeps multi-token shadows (`0 4px 6px red`) and `var(--x)` as values while
        // treating `rgb(0 0 0)` as a color.
        return DataTypeInference.InferDataType(value.Value, [DataType.Color]) != DataType.Color;
    }

    /// <summary>
    /// Resolves an inset-shadow value, prefixing <c>inset</c> as Tailwind does
    /// (<c>inset-shadow-[0_2px_4px_red]</c> → <c>inset 0 2px 4px red</c>).
    /// </summary>
    public static bool TryResolveInsetShadow(CandidateValue value, out string resolved)
    {
        resolved = string.Empty;
        if (!IsArbitraryShadowValue(value))
        {
            return false;
        }

        resolved = value.Value.StartsWith("inset", StringComparison.Ordinal)
            ? value.Value
            : $"inset {value.Value}";
        return true;
    }

    /// <summary>
    /// Resolves a drop-shadow value, wrapping it in <c>drop-shadow()</c> as Tailwind does
    /// (<c>drop-shadow-(--my-shadow)</c> → <c>drop-shadow(var(--my-shadow))</c>).
    /// </summary>
    public static bool TryResolveDropShadow(CandidateValue value, out string resolved)
    {
        resolved = string.Empty;
        if (!IsArbitraryShadowValue(value))
        {
            return false;
        }

        resolved = value.Value.StartsWith("drop-shadow(", StringComparison.Ordinal)
            ? value.Value
            : $"drop-shadow({value.Value})";
        return true;
    }
}
