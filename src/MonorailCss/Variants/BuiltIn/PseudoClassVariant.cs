using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant that applies pseudo-class selectors like :hover, :focus, etc.
/// </summary>
internal class PseudoClassVariant : IVariant
{
    private readonly string _pseudoClass;

    public PseudoClassVariant(string name, string pseudoClass, int weight)
    {
        Name = name;
        _pseudoClass = pseudoClass;
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

        // Apply pseudo-class to the selector
        result = current.TransformSelector(s => s.WithPseudo(_pseudoClass));
        return true;
    }
}

/// <summary>
/// Variant that applies pseudo-element selectors like ::before, ::after, etc.
/// </summary>
internal class PseudoElementVariant : IVariant
{
    private readonly string _pseudoElement;

    public PseudoElementVariant(string name, string pseudoElement, int weight)
    {
        Name = name;
        _pseudoElement = pseudoElement;
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

        // Apply pseudo-element to the selector
        result = current.TransformSelector(s => s.WithPseudo(_pseudoElement));
        return true;
    }
}