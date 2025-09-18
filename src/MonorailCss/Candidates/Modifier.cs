namespace MonorailCss.Candidates;

/// <summary>
/// Represents the type of modifier applied to a CSS candidate.
/// </summary>
public enum ModifierKind
{
    /// <summary>
    /// A named modifier.
    /// </summary>
    Named,

    /// <summary>
    /// An arbitrary modifier.
    /// </summary>
    Arbitrary,
}

/// <summary>
/// Represents a modifier associated with a CSS candidate, defined by its kind and value.
/// </summary>
public record Modifier(ModifierKind Kind, string Value)
{
    /// <summary>
    /// Creates a modifier of kind <see cref="ModifierKind.Named"/> with the specified value.
    /// </summary>
    /// <param name="value">The value of the named modifier.</param>
    /// <returns>A new <see cref="Modifier"/> of kind <see cref="ModifierKind.Named"/> with the specified value.</returns>
    public static Modifier Named(string value) => new(ModifierKind.Named, value);

    /// <summary>
    /// Creates a modifier of kind <see cref="ModifierKind.Arbitrary"/> with the specified value.
    /// </summary>
    /// <param name="value">The value of the arbitrary modifier.</param>
    /// <returns>A new <see cref="Modifier"/> of kind <see cref="ModifierKind.Arbitrary"/> with the specified value.</returns>
    public static Modifier Arbitrary(string value) => new(ModifierKind.Arbitrary, value);

    /// <inheritdoc />
    public override string ToString() => Kind == ModifierKind.Arbitrary ? $"[{Value}]" : Value;
}