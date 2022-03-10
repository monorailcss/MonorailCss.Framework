namespace MonorailCss.Variants;

/// <summary>
/// Represents a variant that applies a pseudo class to an element e.g. .hover\:bg-red becomes .hover\bg-red:hover.
/// </summary>
/// <param name="PseudoClass">The pseudo class to apply.</param>
public record PseudoClassVariant(string PseudoClass) : IVariant;