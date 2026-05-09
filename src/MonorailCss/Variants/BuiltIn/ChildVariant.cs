using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant that targets direct children (<c>*:</c>) or all descendants (<c>**:</c>) of an element.
/// Tailwind v4 emits these as <c>.\*\:foo > *</c> and <c>.\*\*\:foo *</c>.
/// </summary>
internal sealed class ChildVariant : IVariant
{
    private readonly string _combinator;

    public ChildVariant(string name, string combinator, int weight)
    {
        Name = name;
        _combinator = combinator;
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
        if (!CanHandle(token))
        {
            result = current;
            return false;
        }

        var suffix = _combinator.Length == 0 ? " *" : $" {_combinator} *";
        result = current.TransformSelector(s => new Selector(s.Value + suffix));
        return true;
    }
}
