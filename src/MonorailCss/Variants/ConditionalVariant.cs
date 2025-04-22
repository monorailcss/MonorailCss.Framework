namespace MonorailCss.Variants;

/// <summary>
/// Represents a variant that applies a selector conditionally e.g.
/// </summary>
/// <param name="Selector">The selector to prefix.</param>
/// <param name="Condition">The condition.</param>
public record ConditionalVariant(string Selector, string Condition) : IVariant;