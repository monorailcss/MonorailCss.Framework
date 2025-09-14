using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant for container queries (@, @min, @max prefixes).
/// Supports both named and unnamed container queries.
/// </summary>
internal class ContainerQueryVariant : IVariant
{
    private readonly string _containerSize;
    private readonly ContainerQueryType _queryType;
    private readonly Theme.Theme _theme;

    public ContainerQueryVariant(string name, string containerSize, ContainerQueryType queryType, Theme.Theme theme, int weight)
    {
        Name = name;
        _containerSize = containerSize;
        _queryType = queryType;
        _theme = theme;
        Weight = weight;
    }

    public string Name { get; }
    public int Weight { get; }
    public VariantKind Kind => VariantKind.Functional;
    public VariantConstraints Constraints => VariantConstraints.Any;

    public bool CanHandle(VariantToken token)
    {
        // Handle @, @min, @max variants
        if (token.Name == Name)
        {
            // For static variants like @md, @lg (no value needed)
            if (_queryType == ContainerQueryType.Min && token.Value == null)
            {
                return true;
            }

            // For functional variants like @min-md, @max-lg (value required)
            if (_queryType is ContainerQueryType.MinFunctional or ContainerQueryType.MaxFunctional
                && token.Value != null)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryApply(AppliedSelector current, VariantToken token, out AppliedSelector result)
    {
        result = current;

        if (!CanHandle(token))
        {
            return false;
        }

        string? containerWidth = null;

        // Resolve container width based on query type
        switch (_queryType)
        {
            case ContainerQueryType.Min:
                // Static variant like @md - use predefined size
                containerWidth = _theme.ResolveValue(_containerSize, ["--container"]);
                break;

            case ContainerQueryType.MinFunctional:
            case ContainerQueryType.MaxFunctional:
                // Functional variant like @min-md or @max-lg - resolve from token value
                if (token.Value != null)
                {
                    // Try arbitrary value first (e.g., @min-[32rem])
                    if (token.Value.StartsWith('[') && token.Value.EndsWith(']'))
                    {
                        containerWidth = token.Value[1..^1];
                    }
                    else
                    {
                        // Try named value from theme
                        containerWidth = _theme.ResolveValue(token.Value, ["--container"]);
                    }
                }

                break;
        }

        if (containerWidth == null)
        {
            return false;
        }

        // Build container query
        string query;
        var condition = _queryType == ContainerQueryType.MaxFunctional
            ? $"(width < {containerWidth})"
            : $"(width >= {containerWidth})";

        // Handle named containers via modifier
        if (token.Modifier != null)
        {
            query = $"{token.Modifier} {condition}";
        }
        else
        {
            query = condition;
        }

        result = current.WithWrapper(AtRuleWrapper.Container(query));
        return true;
    }
}

/// <summary>
/// Types of container queries supported.
/// </summary>
public enum ContainerQueryType
{
    /// <summary>
    /// Static min-width container query (@md, @lg, etc.)
    /// </summary>
    Min,

    /// <summary>
    /// Functional min-width container query (@min-md, @min-lg, etc.)
    /// </summary>
    MinFunctional,

    /// <summary>
    /// Functional max-width container query (@max-md, @max-lg, etc.)
    /// </summary>
    MaxFunctional,
}