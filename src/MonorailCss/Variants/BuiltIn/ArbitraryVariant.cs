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
            // Relative selector (e.g., [&>*], [&:nth-child(3)])
            result = current.TransformSelector(s => s.Relativize(content));
        }
        else
        {
            // Absolute selector (treated as ancestor)
            result = current.TransformSelector(s => s.DescendantOf(content));
        }

        return true;
    }
}