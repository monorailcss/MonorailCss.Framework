using System.Collections.Immutable;

namespace MonorailCss.Build.Tasks.Parsing;

/// <summary>
/// Represents a parsed custom utility definition from a CSS @utility directive.
/// This is used internally by the parser before conversion to framework DTOs.
/// </summary>
internal record UtilityDefinition
{
    /// <summary>
    /// Gets or sets the name pattern of the utility.
    /// Can be a static name (e.g., "scrollbar-none") or a dynamic pattern (e.g., "scrollbar-thumb-*").
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CSS declarations for this utility.
    /// These are the property/value pairs that will be applied when the utility is used.
    /// </summary>
    public ImmutableList<CssDeclaration> Declarations { get; set; } = ImmutableList<CssDeclaration>.Empty;

    /// <summary>
    /// Gets or sets the nested selectors for this utility.
    /// These are selectors that use the &amp; parent reference (e.g., "&amp;::-webkit-scrollbar").
    /// </summary>
    public ImmutableList<NestedSelector> NestedSelectors { get; set; } = ImmutableList<NestedSelector>.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this utility uses a wildcard pattern.
    /// Wildcard patterns contain "*" and match dynamic values.
    /// </summary>
    public bool IsWildcard { get; set; }

    /// <summary>
    /// Gets or sets the CSS custom properties that this utility depends on.
    /// These are properties that are referenced via var() or set as custom properties.
    /// </summary>
    public ImmutableList<string> CustomPropertyDependencies { get; set; } = ImmutableList<string>.Empty;
}

/// <summary>
/// Represents a CSS declaration (property: value pair).
/// </summary>
internal record CssDeclaration
{
    /// <summary>
    /// Gets or sets the CSS property name.
    /// </summary>
    public string Property { get; set; }

    /// <summary>
    /// Gets or sets the CSS property value.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CssDeclaration"/> class.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="value">The value.</param>
    public CssDeclaration(string property, string value)
    {
        Property = property;
        Value = value;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Property}: {Value}";
}

/// <summary>
/// Represents a nested selector with its declarations.
/// </summary>
internal record NestedSelector(string Selector, ImmutableList<CssDeclaration> Declarations)
{
    /// <summary>
    /// Gets the selector string (e.g., "&amp;::-webkit-scrollbar").
    /// The &amp; represents the parent selector.
    /// </summary>
    public string Selector { get; } = Selector;

    /// <summary>
    /// Gets the CSS declarations within this nested selector.
    /// </summary>
    public ImmutableList<CssDeclaration> Declarations { get; } = Declarations;

    /// <inheritdoc />
    public override string ToString() => $"{Selector} {{ {Declarations.Count} declarations }}";
}