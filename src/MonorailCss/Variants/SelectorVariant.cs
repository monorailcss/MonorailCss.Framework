namespace MonorailCss.Variants;

/// <summary>
/// Represents a variant that applies a selector before the element e.g. dark\:bg-red becomes .dark dark\:bg-red.
/// </summary>
/// <param name="Selector">The selector to prefix.</param>
public record SelectorVariant(string Selector) : IVariant;