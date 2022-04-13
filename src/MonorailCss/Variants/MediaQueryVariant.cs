namespace MonorailCss.Variants;

/// <summary>
/// Represents a variant that is a media query with a feature e.g. .print becomes @media print{ .print { ... } }.
/// </summary>
/// <param name="Feature">The media feature.</param>
/// <param name="Priority">The priority of the media rule.</param>
public record MediaQueryVariant(string Feature, int Priority = 0) : IVariant;