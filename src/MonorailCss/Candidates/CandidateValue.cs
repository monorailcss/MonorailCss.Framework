namespace MonorailCss.Candidates;

/// <summary>
/// Represents the kinds of values used within the Monorail CSS framework.
/// </summary>
public enum ValueKind
{
    /// <summary>
    /// A named value.
    /// </summary>
    Named,

    /// <summary>
    /// An arbitrary value.
    /// </summary>
    Arbitrary,
}

/// <summary>
/// Represents a candidate value with an associated kind and optional fraction.
/// </summary>
/// <remarks>
/// This type is used to distinguish between named and arbitrary values, providing
/// additional precision through the optional fraction for named values, and an
/// optional <see cref="DataTypeHint"/> carried from arbitrary values that used
/// the <c>[type:value]</c> or <c>(type:--var)</c> syntax.
/// </remarks>
public record CandidateValue(
    ValueKind Kind,
    string Value,
    string? Fraction = null,
    string? DataTypeHint = null,
    bool IsParenthesesShorthand = false)
{
    /// <summary>
    /// Creates a new instance of the <see cref="CandidateValue"/> class with the specified kind as Named.
    /// </summary>
    /// <param name="value">The named value to be assigned.</param>
    /// <param name="fraction">An optional fraction to associate with the value.</param>
    /// <returns>A new <see cref="CandidateValue"/> instance of kind Named with the provided value and optional fraction.</returns>
    public static CandidateValue Named(string value, string? fraction = null) => new(ValueKind.Named, value, fraction);

    /// <summary>
    /// Creates a new instance of the <see cref="CandidateValue"/> class with the specified kind as Arbitrary.
    /// </summary>
    /// <param name="value">The arbitrary value to be assigned.</param>
    /// <param name="dataTypeHint">Optional Tailwind type hint (e.g. <c>color</c>, <c>length</c>) parsed from <c>[type:value]</c>.</param>
    /// <param name="isParenthesesShorthand">True when the source syntax was <c>(--var)</c> rather than <c>[var(--var)]</c>.</param>
    /// <returns>A new <see cref="CandidateValue"/> instance of kind Arbitrary with the provided value.</returns>
    public static CandidateValue Arbitrary(string value, string? dataTypeHint = null, bool isParenthesesShorthand = false) =>
        new(ValueKind.Arbitrary, value, null, dataTypeHint, isParenthesesShorthand);

    /// <inheritdoc />
    public override string ToString() => Kind == ValueKind.Arbitrary ? $"[{Value}]" : Value;
}