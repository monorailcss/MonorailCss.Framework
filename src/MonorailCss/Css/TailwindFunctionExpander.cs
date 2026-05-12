using System.Collections.Immutable;
using MonorailCss.Ast;

namespace MonorailCss.Css;

/// <summary>
/// Expands Tailwind v4 build-time CSS function macros that aren't real CSS:
/// <list type="bullet">
///   <item><c>--alpha(A / B)</c> → <c>color-mix(in oklab, A B, transparent)</c></item>
///   <item><c>--spacing(N)</c> → <c>calc(var(--spacing) * N)</c></item>
/// </list>
/// Browsers don't understand these — Tailwind resolves them at compile time. We do the same
/// at the string-rewrite layer so the output is portable CSS regardless of which utility
/// produced the declaration.
/// </summary>
internal static class TailwindFunctionExpander
{
    /// <summary>
    /// Applies every supported expansion to <paramref name="value"/> until a fixed point.
    /// Idempotent: <c>Expand(Expand(x)) == Expand(x)</c>.
    /// </summary>
    public static string Expand(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        // Repeated passes: a --spacing() inside an --alpha() needs the inner to expand
        // before the outer split-on-'/' walks the argument.
        for (var i = 0; i < 8; i++)
        {
            var next = ExpandOnce(value);
            if (ReferenceEquals(next, value))
            {
                return value;
            }

            value = next;
        }

        return value;
    }

    /// <summary>
    /// Walks an AST tree and rewrites every <see cref="Declaration"/> value via <see cref="Expand"/>.
    /// Returns the same list reference if nothing changed.
    /// </summary>
    public static ImmutableList<AstNode> ExpandTree(ImmutableList<AstNode> nodes)
    {
        var builder = ImmutableList.CreateBuilder<AstNode>();
        var modified = false;

        foreach (var node in nodes)
        {
            var rewritten = ExpandNode(node);
            builder.Add(rewritten);
            if (!ReferenceEquals(rewritten, node))
            {
                modified = true;
            }
        }

        return modified ? builder.ToImmutable() : nodes;
    }

    private static AstNode ExpandNode(AstNode node)
    {
        switch (node)
        {
            case Declaration declaration:
                var expanded = Expand(declaration.Value);
                return ReferenceEquals(expanded, declaration.Value)
                    ? declaration
                    : declaration with { Value = expanded };

            case StyleRule styleRule:
                var styleNodes = ExpandTree(styleRule.Nodes);
                return ReferenceEquals(styleNodes, styleRule.Nodes)
                    ? styleRule
                    : styleRule with { Nodes = styleNodes };

            case NestedRule nestedRule:
                var nestedNodes = ExpandTree(nestedRule.Nodes);
                return ReferenceEquals(nestedNodes, nestedRule.Nodes)
                    ? nestedRule
                    : nestedRule with { Nodes = nestedNodes };

            case AtRule atRule:
                var atNodes = ExpandTree(atRule.Nodes);
                return ReferenceEquals(atNodes, atRule.Nodes)
                    ? atRule
                    : atRule with { Nodes = atNodes };

            case Context contextNode:
                var contextNodes = ExpandTree(contextNode.Nodes);
                return ReferenceEquals(contextNodes, contextNode.Nodes)
                    ? contextNode
                    : contextNode with { Nodes = contextNodes };

            default:
                return node;
        }
    }

    private static string ExpandOnce(string value)
    {
        // Cheap path: no expandable function present.
        if (value.IndexOf("--alpha(", StringComparison.Ordinal) < 0
            && value.IndexOf("--spacing(", StringComparison.Ordinal) < 0)
        {
            return value;
        }

        // Pick the LEFTMOST occurrence between --alpha( and --spacing( and rewrite that one.
        // The loop in Expand() will re-enter to catch the next one and any newly-revealed
        // inner functions.
        var alphaIdx = value.IndexOf("--alpha(", StringComparison.Ordinal);
        var spacingIdx = value.IndexOf("--spacing(", StringComparison.Ordinal);

        if (alphaIdx >= 0 && (spacingIdx < 0 || alphaIdx < spacingIdx))
        {
            return TryRewriteAlphaAt(value, alphaIdx, out var rewritten) ? rewritten : value;
        }

        if (spacingIdx >= 0)
        {
            return TryRewriteSpacingAt(value, spacingIdx, out var rewritten) ? rewritten : value;
        }

        return value;
    }

    private static bool TryRewriteAlphaAt(string value, int start, out string result)
    {
        var openParen = start + "--alpha".Length;
        if (!TryFindMatchingParen(value, openParen, out var closeParen))
        {
            result = value;
            return false;
        }

        var inner = value.Substring(openParen + 1, closeParen - openParen - 1);
        if (!TrySplitTopLevel(inner, '/', out var colorPart, out var opacityPart))
        {
            // --alpha(...) without a top-level '/' isn't valid input — leave it alone so the
            // user sees the original problem rather than a silent rewrite.
            result = value;
            return false;
        }

        var replacement = $"color-mix(in oklab, {colorPart.Trim()} {opacityPart.Trim()}, transparent)";
        result = string.Concat(
            value.AsSpan(0, start),
            replacement,
            value.AsSpan(closeParen + 1));
        return true;
    }

    private static bool TryRewriteSpacingAt(string value, int start, out string result)
    {
        var openParen = start + "--spacing".Length;
        if (!TryFindMatchingParen(value, openParen, out var closeParen))
        {
            result = value;
            return false;
        }

        var inner = value.Substring(openParen + 1, closeParen - openParen - 1).Trim();
        if (inner.Length == 0)
        {
            result = value;
            return false;
        }

        var replacement = $"calc(var(--spacing) * {inner})";
        result = string.Concat(
            value.AsSpan(0, start),
            replacement,
            value.AsSpan(closeParen + 1));
        return true;
    }

    /// <summary>
    /// Given a string and the index of an opening <c>(</c>, returns the index of the matching <c>)</c>.
    /// Handles nested parentheses but ignores quoted content (covers <c>url("...)")</c>-style edge cases).
    /// </summary>
    private static bool TryFindMatchingParen(string value, int openIndex, out int closeIndex)
    {
        closeIndex = -1;
        if (openIndex < 0 || openIndex >= value.Length || value[openIndex] != '(')
        {
            return false;
        }

        var depth = 1;
        char? quote = null;
        for (var i = openIndex + 1; i < value.Length; i++)
        {
            var c = value[i];

            if (quote != null)
            {
                if (c == quote && value[i - 1] != '\\')
                {
                    quote = null;
                }

                continue;
            }

            switch (c)
            {
                case '"':
                case '\'':
                    quote = c;
                    break;
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    if (depth == 0)
                    {
                        closeIndex = i;
                        return true;
                    }

                    break;
            }
        }

        return false;
    }

    /// <summary>
    /// Splits <paramref name="value"/> on the first occurrence of <paramref name="separator"/>
    /// that is not inside any parenthesized group or quoted span. Returns false if the separator
    /// does not appear at the top level.
    /// </summary>
    private static bool TrySplitTopLevel(string value, char separator, out string left, out string right)
    {
        var depth = 0;
        char? quote = null;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (quote != null)
            {
                if (c == quote && (i == 0 || value[i - 1] != '\\'))
                {
                    quote = null;
                }

                continue;
            }

            switch (c)
            {
                case '"':
                case '\'':
                    quote = c;
                    break;
                case '(':
                    depth++;
                    break;
                case ')':
                    if (depth > 0)
                    {
                        depth--;
                    }

                    break;
                default:
                    if (depth == 0 && c == separator)
                    {
                        left = value.Substring(0, i);
                        right = value.Substring(i + 1);
                        return true;
                    }

                    break;
            }
        }

        left = value;
        right = string.Empty;
        return false;
    }
}
