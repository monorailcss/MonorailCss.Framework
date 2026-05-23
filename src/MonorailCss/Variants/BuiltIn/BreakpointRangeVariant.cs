using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant for functional responsive breakpoints: <c>min-*</c> and <c>max-*</c>.
/// The value is either an arbitrary length (<c>min-[1100px]</c>) or a named breakpoint resolved
/// from the <c>--breakpoint-*</c> theme namespace (<c>min-tablet</c>, <c>max-lg</c>).
/// Output matches Tailwind v4: <c>min-*</c> emits <c>@media (min-width: X)</c> and <c>max-*</c>
/// emits <c>@media not all and (min-width: X)</c> (exclusive upper bound).
/// </summary>
internal sealed class BreakpointRangeVariant : IVariant
{
    private readonly bool _isMax;
    private readonly Theme.Theme _theme;

    public BreakpointRangeVariant(string name, bool isMax, Theme.Theme theme, int weight)
    {
        Name = name;
        _isMax = isMax;
        _theme = theme;
        Weight = weight;
    }

    public string Name { get; }

    public int Weight { get; }

    public VariantKind Kind => VariantKind.Functional;

    public VariantConstraints Constraints => VariantConstraints.Any;

    public bool CanHandle(VariantToken token) =>
        token.Name == Name && token.Value != null;

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        var value = token.Value!;

        // Arbitrary length (e.g. min-[1100px]) takes the literal inside the brackets;
        // otherwise resolve a named breakpoint from the --breakpoint-* theme namespace.
        string? width;
        if (value.StartsWith('[') && value.EndsWith(']'))
        {
            width = value[1..^1];
        }
        else
        {
            width = _theme.ResolveValue(value, ["--breakpoint"]);
        }

        if (string.IsNullOrEmpty(width))
        {
            return false;
        }

        var query = _isMax
            ? $"not all and (min-width: {width})"
            : $"(min-width: {width})";

        result = current.WithWrapper(AtRuleWrapper.Media(query));
        return true;
    }
}
