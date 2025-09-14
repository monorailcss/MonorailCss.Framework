using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant for CSS @supports feature queries.
/// Handles supports-[...] syntax.
/// </summary>
internal partial class SupportsVariant : IVariant
{
    public SupportsVariant(int weight)
    {
        Weight = weight;
    }

    public string Name => "supports";
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Functional;
    public VariantConstraints Constraints => VariantConstraints.Any;

    public bool CanHandle(VariantToken token)
    {
        return token.Name == "supports" && !string.IsNullOrEmpty(token.Value);
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        var value = token.Value!;

        // Remove brackets if present
        if (value.StartsWith('[') && value.EndsWith(']'))
        {
            value = value[1..^1];
        }

        // When the value starts with functions like not(), selector(), font-tech(), etc.
        // we can use the value as-is
        if (FunctionStartRegex().IsMatch(value))
        {
            // Chrome has a bug where `(condition1)or(condition2)` is not valid, but
            // `(condition1) or (condition2)` is supported.
            var query = ChromeFunctionFallbackRegex().Replace(value, " $1 ").Trim();

            // Also clean up any extra spaces
            query = ExtraSpaceRegex().Replace(query, " ");
            result = current.WithWrapper(new AtRuleWrapper("supports", query));
            return true;
        }

        // When `supports-[display]` is used as a shorthand, we need to make sure
        // that this becomes a valid CSS supports condition.
        // E.g.: `supports-[display]` -> `@supports (display: var(--tw))`
        if (!value.Contains(':'))
        {
            value = $"{value}: var(--tw)";
        }

        // When `supports-[display:flex]` is used, we need to make sure that this
        // becomes a valid CSS supports condition by wrapping it in parens.
        // E.g.: `supports-[display:flex]` -> `@supports (display: flex)`
        if (!value.StartsWith('(') || !value.EndsWith(')'))
        {
            value = $"({value})";
        }

        result = current.WithWrapper(new AtRuleWrapper("supports", value));
        return true;
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"^[\w-]*\s*\(")]
    private static partial System.Text.RegularExpressions.Regex FunctionStartRegex();
    [System.Text.RegularExpressions.GeneratedRegex(@"\b(and|or|not)\b")]
    private static partial System.Text.RegularExpressions.Regex ChromeFunctionFallbackRegex();
    [System.Text.RegularExpressions.GeneratedRegex(@"\s+")]
    private static partial System.Text.RegularExpressions.Regex ExtraSpaceRegex();
}