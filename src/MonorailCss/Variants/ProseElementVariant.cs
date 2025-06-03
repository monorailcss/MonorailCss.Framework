namespace MonorailCss.Variants;

/// <summary>
/// Represents a variant specifically for prose element targets, where the selector appears after the class name.
/// </summary>
/// <param name="Selector">The selector to append.</param>
public record ProseElementVariant(string Selector) : IVariant;
