using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant for custom selector patterns defined via @custom-variant directives.
/// Supports complex selector patterns including ampersand (&amp;) references.
/// </summary>
internal class CustomSelectorVariant : IVariant
{
    private readonly string _selectorPattern;

    public CustomSelectorVariant(string name, string selectorPattern, int weight)
    {
        Name = name;
        _selectorPattern = selectorPattern;
        Weight = weight;
    }

    public string Name { get; }
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Static;
    public VariantConstraints Constraints => VariantConstraints.StyleRules;

    public bool CanHandle(VariantToken token)
    {
        return token.Name == Name && token.Value == null && token.Modifier == null;
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        // The selector pattern should contain & to indicate where the base selector goes
        // Example: (&:where(.dark, .dark *)) or (&::-webkit-scrollbar)
        if (_selectorPattern.Contains('&'))
        {
            // Use Relativize for patterns with & reference
            result = current.TransformSelector(s => s.Relativize(_selectorPattern));
        }
        else
        {
            // For patterns without &, treat as ancestor/descendant relationship
            // This is less common but possible
            result = current.TransformSelector(s => s.DescendantOf(_selectorPattern));
        }

        return true;
    }
}
