using System.Collections.Immutable;
using MonorailCss.Variants;

namespace MonorailCss.Candidates;

/// <summary>
/// Represents an abstract base record for CSS candidates such as utilities or properties.
/// A candidate is a reference to a CSS utility or property that can potentially be compiled or processed.
/// </summary>
/// <remarks>
/// The <see cref="Candidate"/> class contains fields necessary to identify, classify, and process CSS candidates
/// such as the raw utility name, associated variants, modifiers, and other metadata.
/// </remarks>
public abstract record Candidate
{
    /// <summary>
    /// Gets the raw string representation of a candidate.
    /// </summary>
    /// <remarks>
    /// The <c>Raw</c> property represents the unprocessed or original format of the candidate.
    /// It is typically used as the primary string identifier or input for logic that processes or evaluates
    /// utility generation within styling frameworks or systems.
    /// </remarks>
    public required string Raw { get; init; }

    /// <summary>
    /// Gets the collection of variant tokens associated with a candidate.
    /// </summary>
    /// <remarks>
    /// The <c>Variants</c> property represents a set of modifiers or contextual tokens
    /// that define variations or specific conditions under which a candidate may be applied.
    /// These tokens are typically used for generating context-aware styles or utilities,
    /// and they are a fundamental component in building conditional styling frameworks.
    /// </remarks>
    public required ImmutableArray<VariantToken> Variants { get; init; }

    /// <summary>
    /// Gets the optional modifier for the candidate.
    /// </summary>
    /// <remarks>
    /// The <c>Modifier</c> property defines an additional qualifier or specification associated with
    /// a CSS candidate. It may represent named or arbitrary modifiers, providing extra context or
    /// customization for styling purposes. This property is nullable, indicating that a candidate may
    /// not require a modifier.
    /// </remarks>
    public Modifier? Modifier { get; init; }

    /// <summary>
    /// Gets a value indicating whether the candidate should be treated as important.
    /// </summary>
    /// <remarks>
    /// The <c>Important</c> property specifies whether the candidate's resulting CSS declarations
    /// should include the <c>!important</c> modifier. This ensures that the corresponding styles
    /// take precedence over others with the same specificity.
    /// </remarks>
    public bool Important { get; init; }

    /// <summary>
    /// Gets the normalized string representation of a candidate.
    /// </summary>
    /// <remarks>
    /// The <c>Normalized</c> property represents a processed or standardized form of a candidate,
    /// typically used to ensure consistent comparison or ordering within systems. It combines components
    /// like variants, modifiers, and importance in a uniform representation, facilitating better handling
    /// during operations like sorting or utility processing.
    /// </remarks>
    public string? Normalized { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Candidate"/> class.
    /// </summary>
    protected Candidate()
    {
        Variants = ImmutableArray<VariantToken>.Empty;
    }
}

/// <summary>
/// Represents a static utility in the MonorailCSS framework, which is a specific type of CSS candidate
/// that maps directly to a predefined static CSS property or declaration.
/// </summary>
/// <remarks>
/// The <see cref="StaticUtility"/> record extends the <see cref="Candidate"/> class by adding
/// a <c>Root</c> property, which identifies the core part of the utility's name used for compilation.
/// Static utilities are generally processed based on fixed mappings between their root names and
/// corresponding CSS properties and values.
/// </remarks>
public record StaticUtility : Candidate
{
    /// <summary>
    /// Gets the root identifier of a static utility in the MonorailCSS framework.
    /// </summary>
    /// <remarks>
    /// The <c>Root</c> property represents the core part of the utility's name, which is used as the basis
    /// for mapping the utility to a corresponding CSS property or declaration during compilation.
    /// This property is critical for distinguishing and processing static utilities within the framework.
    /// </remarks>
    public required string Root { get; init; }

    /// <inheritdoc />
    public override string ToString() => $"StaticUtility({Root})";
}

/// <summary>
/// Represents a functional CSS utility that consists of a root and an optional candidate value.
/// </summary>
/// <remarks>
/// The <see cref="FunctionalUtility"/> is a specialized type of <see cref="Candidate"/> that holds
/// a functional root identifier, such as a CSS utility prefix, and optionally a value that describes
/// the specific configuration or variant of the utility. This provides a mechanism for parsing and processing
/// structured CSS utilities.
/// </remarks>
public record FunctionalUtility : Candidate
{
    /// <summary>
    /// Gets the root prefix for a functional utility.
    /// </summary>
    /// <remarks>
    /// The <c>Root</c> property defines the base or starting segment of a utility's CSS class name.
    /// It is primarily used to signify the class's primary category or functional purpose,
    /// such as "bg" for background-related utilities. This enables structured generation
    /// of CSS classes within predefined styling systems.
    /// </remarks>
    public required string Root { get; init; }

    /// <summary>
    /// Gets the processed representation of the candidate's value.
    /// </summary>
    /// <remarks>
    /// The <c>Value</c> property encapsulates the specific details of the candidate,
    /// including its kind and associated string value. This value is utilized throughout
    /// various processing stages to determine the functional or stylistic significance
    /// of the candidate within the framework.
    /// </remarks>
    public required CandidateValue? Value { get; init; }

    /// <inheritdoc />
    public override string ToString() => Value != null ? $"FunctionalUtility({Root}-{Value})" : $"FunctionalUtility({Root})";
}

/// <summary>
/// Represents an arbitrary CSS property defined by a custom property-value pair.
/// </summary>
/// <remarks>
/// The <see cref="ArbitraryProperty"/> class is a specialization of the <see cref="Candidate"/>
/// abstract record. It is used to model CSS properties that are explicitly defined with
/// arbitrary values, typically expressed in a "[property:value]" format.
/// This class is particularly useful for representing non-standard or dynamically defined
/// CSS that does not correspond to predefined utilities or static functional structures.
/// </remarks>
public record ArbitraryProperty : Candidate
{
    /// <summary>
    /// Gets the property name for an arbitrary CSS declaration.
    /// </summary>
    /// <remarks>
    /// The <c>Property</c> represents the CSS property name associated with an arbitrary declaration. It is used
    /// in conjunction with its corresponding value to define custom or non-standard CSS styles.
    /// This property must adhere to CSS naming conventions and should not start with an uppercase character.
    /// </remarks>
    public required string Property { get; init; }

    /// <summary>
    /// Gets the value associated with the arbitrary property.
    /// </summary>
    /// <remarks>
    /// The <c>Value</c> property represents the content assigned to a CSS property,
    /// in its unprocessed state, for utility generation or validation.
    /// This value may be transformed or validated during processing to ensure
    /// compatibility and correctness within the styling framework.
    /// </remarks>
    public required string Value { get; init; }

    /// <inheritdoc />
    public override string ToString() => $"ArbitraryProperty([{Property}:{Value}])";
}