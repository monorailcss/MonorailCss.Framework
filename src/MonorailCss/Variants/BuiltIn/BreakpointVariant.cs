using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant for responsive breakpoint media queries (sm, md, lg, xl, 2xl).
/// </summary>
internal class BreakpointVariant : IVariant
{
    private readonly string _mediaQuery;

    public BreakpointVariant(string name, string mediaQuery, int weight)
    {
        Name = name;
        _mediaQuery = mediaQuery;
        Weight = weight;
    }

    public string Name { get; }
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Static;
    public VariantConstraints Constraints => VariantConstraints.Any;

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

        // Add media query wrapper
        result = current.WithWrapper(AtRuleWrapper.Media(_mediaQuery));
        return true;
    }
}