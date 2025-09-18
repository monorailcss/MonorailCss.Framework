namespace MonorailCss.Variants;

/// <summary>
/// Represents a parsed variant token from a class string.
/// </summary>
public readonly record struct VariantToken(
    string Name,
    string? Modifier,
    string? Value,
    string Raw)
{
    /// <summary>
    /// Creates a simple static variant token.
    /// </summary>
    /// <param name="name">The name of the variant.</param>
    /// <returns>A new instance of <see cref="VariantToken"/> representing the static variant.</returns>
    public static VariantToken Static(string name) =>
        new(name, null, null, name);

    /// <summary>
    /// Creates a functional variant token with a value.
    /// </summary>
    /// <param name="name">The name of the variant.</param>
    /// <param name="value">The value associated with the variant.</param>
    /// <returns>A new instance of <see cref="VariantToken"/> representing the functional variant.</returns>
    public static VariantToken Functional(string name, string value) =>
        new(name, null, value, $"{name}-{value}");

    /// <summary>
    /// Creates an arbitrary variant token based on the given selector.
    /// </summary>
    /// <param name="selector">The arbitrary selector to create the variant token from.</param>
    /// <returns>A new instance of <see cref="VariantToken"/> representing the arbitrary variant.</returns>
    public static VariantToken Arbitrary(string selector) =>
        new("arbitrary", null, selector, $"[{selector}]");

    /// <summary>
    /// Creates a compound variant token.
    /// </summary>
    /// <param name="root">The root name of the compound variant.</param>
    /// <param name="subVariant">The sub-variant name within the compound variant.</param>
    /// <param name="modifier">An optional modifier for the compound variant.</param>
    /// <returns>A new instance of <see cref="VariantToken"/> representing the compound variant.</returns>
    public static VariantToken Compound(string root, string subVariant, string? modifier = null) =>
        new(root, modifier, subVariant, modifier != null ? $"{root}/{modifier}-{subVariant}" : $"{root}-{subVariant}");

    /// <summary>
    /// Gets a value indicating whether checks if this is a media query variant.
    /// </summary>
    public bool IsMediaQuery => Raw.StartsWith("[@media") || Name is "sm" or "md" or "lg" or "xl" or "2xl";

    /// <summary>
    /// Gets a value indicating whether checks if this is a container query variant.
    /// </summary>
    public bool IsContainerQuery => Raw.StartsWith("[@container") || Name == "container";

    /// <summary>
    /// Gets a value indicating whether checks if this is a supports query variant.
    /// </summary>
    public bool IsSupportsQuery => Raw.StartsWith("[@supports");

    /// <summary>
    /// Gets a value indicating whether checks if this is an arbitrary variant.
    /// </summary>
    public bool IsArbitrary => Name == "arbitrary" || (Raw.StartsWith("[") && Raw.EndsWith("]"));

    /// <inheritdoc />
    public override string ToString() => Raw;
}