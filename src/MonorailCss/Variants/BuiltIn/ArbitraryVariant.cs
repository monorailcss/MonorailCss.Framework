using System.Text;
using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant for arbitrary selectors and at-rules.
/// </summary>
internal class ArbitraryVariant : IVariant
{
    public ArbitraryVariant(int weight)
    {
        Weight = weight;
    }

    public string Name => "arbitrary";
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Arbitrary;
    public VariantConstraints Constraints => VariantConstraints.Any;

    public bool CanHandle(VariantToken token)
    {
        return token.IsArbitrary || token.Name == "arbitrary";
    }

    /// <summary>
    /// Determines if the content requires CSS nesting based on whether &amp; is followed by a combinator.
    /// Combinators: &gt; (child), + (adjacent sibling), ~ (general sibling), space (descendant).
    /// </summary>
    private static bool NeedsCssNesting(string content)
    {
        var ampIndex = content.IndexOf('&');
        if (ampIndex == -1 || ampIndex >= content.Length - 1)
        {
            return false;
        }

        var nextChar = content[ampIndex + 1];

        // Check for combinators: >, +, ~, or space
        return nextChar == '>' || nextChar == '+' || nextChar == '~' || nextChar == ' ';
    }

    /// <summary>
    /// Decodes the arbitrary-value underscore convention: an unescaped <c>_</c> becomes a space
    /// (so <c>[&amp;_svg]</c> targets the descendant <c>&amp; svg</c> and <c>[@media_screen]</c>
    /// reads as <c>@media screen</c>), while <c>\_</c> is preserved as a literal underscore.
    /// No-ops when there is no underscore.
    /// </summary>
    private static string DecodeUnderscores(string content)
    {
        if (!content.Contains('_'))
        {
            return content;
        }

        var sb = new StringBuilder(content.Length);
        for (var i = 0; i < content.Length; i++)
        {
            var c = content[i];
            if (c == '\\' && i + 1 < content.Length && content[i + 1] == '_')
            {
                sb.Append('_');
                i++;
            }
            else if (c == '_')
            {
                sb.Append(' ');
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        var content = token.Value ?? token.Raw;

        // Remove brackets if present
        if (content.StartsWith('[') && content.EndsWith(']'))
        {
            content = content[1..^1];
        }

        // Underscores decode to spaces (the standard arbitrary-value rule) before we decide
        // between nesting and a flat selector, so `[&_svg]` reads as the descendant `& svg`.
        content = DecodeUnderscores(content);

        // Check if it's an at-rule
        if (content.StartsWith('@'))
        {
            // Parse at-rule (e.g., @media (min-width: 768px))
            var spaceIndex = content.IndexOf(' ');
            if (spaceIndex > 0)
            {
                var atRuleName = content[1..spaceIndex];
                var atRuleParams = content[(spaceIndex + 1)..];

                result = current.WithWrapper(new AtRuleWrapper(atRuleName, atRuleParams));
                return true;
            }
        }

        // Otherwise, it's a selector modifier
        if (content.Contains('&'))
        {
            // Check if we need CSS nesting (when & is followed by a combinator)
            // Combinators: > (child), + (adjacent sibling), ~ (general sibling), space (descendant)
            if (NeedsCssNesting(content))
            {
                // CSS nesting for combinators (e.g., [&>:first-child], [&+.sibling], [&_svg])
                result = current.TransformSelector(s => s.AsNestedSelector(content));
            }
            else
            {
                // Flat selector (e.g., [&:hover], data-[selected=true] becomes &[data-selected="true"])
                result = current.TransformSelector(s => s.Relativize(content));
            }
        }
        else
        {
            // Absolute selector (treated as ancestor)
            result = current.TransformSelector(s => s.DescendantOf(content));
        }

        return true;
    }
}
