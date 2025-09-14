using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant for functional attribute selectors like data-[], aria-[], has-[].
/// </summary>
internal class FunctionalAttributeVariant : IVariant
{
    private readonly string _name;
    private readonly int _weight;

    public FunctionalAttributeVariant(string name, int weight)
    {
        _name = name;
        _weight = weight;
    }

    public string Name => _name;
    public int Weight => _weight;
    public VariantKind Kind => VariantKind.Functional;
    public VariantConstraints Constraints => VariantConstraints.Any;

    public bool CanHandle(VariantToken token)
    {
        return token.Name == _name && token.Value != null;
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        var value = token.Value ?? string.Empty;

        switch (_name)
        {
            case "data":
                // Transform data-[selected=true] to &[data-selected="true"]
                result = current.TransformSelector(s => s.Relativize($"&[data-{value}]"));
                return true;

            case "aria":
                // Transform aria-[checked] to &[aria-checked="true"]
                // Transform aria-[label=Hello] to &[aria-label="Hello"]
                if (value.Contains('='))
                {
                    result = current.TransformSelector(s => s.Relativize($"&[aria-{value}]"));
                }
                else
                {
                    result = current.TransformSelector(s => s.Relativize($"&[aria-{value}=\"true\"]"));
                }

                return true;

            case "has":
                // Transform has-[.active] to &:has(.active)
                result = current.TransformSelector(s => s.Relativize($"&:has({value})"));
                return true;

            case "where":
                // Transform where-[.active] to &:where(.active)
                result = current.TransformSelector(s => s.Relativize($"&:where({value})"));
                return true;

            case "is":
                // Transform is-[.active] to &:is(.active)
                result = current.TransformSelector(s => s.Relativize($"&:is({value})"));
                return true;

            case "not":
                // Transform not-[.active] to &:not(.active)
                result = current.TransformSelector(s => s.Relativize($"&:not({value})"));
                return true;

            default:
                return false;
        }
    }
}