using System.Collections.Immutable;

namespace MonorailCss.Merging;

/// <summary>
/// Decomposes a shorthand utility class-prefix into the longhand class-prefixes it breaks down to,
/// one level along the natural physical axis. This is the inverse direction of
/// <see cref="ShorthandExpansion"/>: that one expands a declared CSS <em>property</em> into the
/// conflict keys it covers, while this one maps a class <em>root</em> (e.g. <c>my</c>) to its
/// immediate child roots (<c>mt</c>, <c>mb</c>) so <see cref="ClassMerger"/> can rewrite a partially
/// overridden shorthand into the longhand classes that survive — <c>my-4 mt-6</c> becomes
/// <c>mb-4 mt-6</c> rather than leaving the ambiguous pair.
/// </summary>
/// <remarks>
/// One level only: each root maps to its <em>immediate</em> coarser children, and leaf roots
/// (<c>mt</c>, <c>left</c>, <c>w</c>, <c>rounded-tl</c>, …) have no entry. Decomposition recurses
/// through the table, so a deeper conflict only breaks down the sub-axis it touches and recursion
/// terminates at the leaves. Scope is the functional axis families whose children map cleanly to a
/// single physical side; static-utility shorthands (<c>overflow</c>, <c>overscroll</c>,
/// <c>place-*</c>) and the overloaded <c>border</c> width root are intentionally absent and keep the
/// plain superset-merge behavior. Every child listed here is a real utility pattern, so a synthesized
/// child token always re-parses.
/// </remarks>
internal static class ShorthandDecomposition
{
    private static readonly ImmutableDictionary<string, ImmutableArray<string>> _children = BuildTable();

    /// <summary>
    /// Returns the immediate child class-prefixes a shorthand root decomposes into along the
    /// physical axis, or <see langword="null"/> when the root is a leaf or not a recognized
    /// decomposable shorthand.
    /// </summary>
    /// <param name="root">The positive utility root (no leading <c>-</c>), e.g. <c>my</c>.</param>
    /// <returns>The immediate child roots, or null when the root does not decompose.</returns>
    public static ImmutableArray<string>? GetChildPrefixes(string root) =>
        _children.TryGetValue(root, out var children) ? children : null;

    private static ImmutableDictionary<string, ImmutableArray<string>> BuildTable()
    {
        var table = new Dictionary<string, ImmutableArray<string>>(StringComparer.Ordinal);

        // Box families share the {B, Bx, By, Bl, Br, Bt, Bb} class-prefix shape.
        AddBoxFamily(table, "m");
        AddBoxFamily(table, "p");
        AddBoxFamily(table, "scroll-m");
        AddBoxFamily(table, "scroll-p");

        // Inset decomposes through axis roots to the bare physical side roots.
        table["inset"] = ["inset-x", "inset-y"];
        table["inset-x"] = ["left", "right"];
        table["inset-y"] = ["top", "bottom"];

        table["gap"] = ["gap-x", "gap-y"];
        table["size"] = ["w", "h"];

        // Border-radius: rows then corners — rounded-t/rounded-b together cover all four corners.
        table["rounded"] = ["rounded-t", "rounded-b"];
        table["rounded-t"] = ["rounded-tl", "rounded-tr"];
        table["rounded-b"] = ["rounded-bl", "rounded-br"];

        return table.ToImmutableDictionary(StringComparer.Ordinal);
    }

    private static void AddBoxFamily(Dictionary<string, ImmutableArray<string>> table, string b)
    {
        table[b] = [$"{b}x", $"{b}y"];
        table[$"{b}x"] = [$"{b}l", $"{b}r"];
        table[$"{b}y"] = [$"{b}t", $"{b}b"];
    }
}
