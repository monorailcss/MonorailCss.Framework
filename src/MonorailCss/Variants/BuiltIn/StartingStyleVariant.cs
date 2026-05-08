using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant that wraps the rule in <c>@starting-style { ... }</c>. Used to declare the
/// starting state of a transition for elements that animate when entering the DOM
/// (e.g. <c>&lt;dialog&gt;</c>'s <c>::backdrop</c> fading in).
/// </summary>
internal sealed class StartingStyleVariant : IVariant
{
    public StartingStyleVariant(int weight)
    {
        Weight = weight;
    }

    public string Name => "starting";

    public int Weight { get; }

    public VariantKind Kind => VariantKind.Static;

    public VariantConstraints Constraints => VariantConstraints.Any;

    public bool CanHandle(VariantToken token) =>
        token.Name == Name && token.Value == null && token.Modifier == null;

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        result = current.WithWrapper(new AtRuleWrapper("starting-style", string.Empty));
        return true;
    }
}
