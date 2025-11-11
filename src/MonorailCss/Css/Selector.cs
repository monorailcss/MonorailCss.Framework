namespace MonorailCss.Css;

/// <summary>
/// Represents a CSS selector and provides methods for manipulating and combining selectors.
/// </summary>
public record Selector(string Value)
{
    /// <summary>
    /// Gets the nested selector pattern that should be applied using CSS nesting.
    /// When present, indicates this selector should generate nested CSS rules.
    /// </summary>
    public string? NestedSelector { get; init; }

    /// <summary>
    /// Creates a selector from an escaped class name.
    /// </summary>
    /// <param name="escapedClassName">The escaped class name to use for creating the selector.</param>
    /// <returns>A new <see cref="Selector"/> instance with the given escaped class name.</returns>
    public static Selector FromClass(string escapedClassName) => new($".{escapedClassName}");

    /// <summary>
    /// Appends a pseudo-class or pseudo-element to the current selector.
    /// </summary>
    /// <param name="pseudo">The pseudo-class or pseudo-element to append to the selector (e.g., ":hover", ":active").</param>
    /// <returns>A new <see cref="Selector"/> instance with the appended pseudo-class or pseudo-element.</returns>
    public Selector WithPseudo(string pseudo) => new($"{Value}{pseudo}");

    /// <summary>
    /// Replaces the first occurrence of "&amp;" in the given relative selector with the value of the current selector.
    /// </summary>
    /// <param name="relative">The relative selector string where "&amp;" will be replaced.</param>
    /// <returns>A new <see cref="Selector"/> instance with the updated selector string.</returns>
    public Selector Relativize(string relative) => new(relative.Replace("&", Value));

    /// <summary>
    /// Creates a selector with CSS nesting pattern for use with combinators (e.g., "&gt;:first-child", "&amp;+.sibling").
    /// The nested pattern will be rendered using CSS nesting syntax with the &amp; parent selector.
    /// </summary>
    /// <param name="nestedPattern">The nested selector pattern containing combinators after &amp;.</param>
    /// <returns>A new <see cref="Selector"/> instance with the nested selector pattern set.</returns>
    public Selector AsNestedSelector(string nestedPattern) => new(Value) { NestedSelector = nestedPattern };

    /// <summary>
    /// Makes this selector a descendant of the specified ancestor selector.
    /// </summary>
    /// <param name="ancestor">The ancestor selector to which this selector will become a descendant.</param>
    /// <returns>A new <see cref="Selector"/> instance representing the descendant relationship.</returns>
    public Selector DescendantOf(string ancestor) => new($"{ancestor} {Value}");

    /// <summary>
    /// Creates a selector that represents a sibling relationship with another selector using the specified combinator.
    /// </summary>
    /// <param name="sibling">The selector to which this selector will be a sibling.</param>
    /// <param name="combinator">The combinator that defines the type of sibling relationship (e.g., "~" for general sibling, "+" for adjacent sibling).</param>
    /// <returns>A new <see cref="Selector"/> instance representing the sibling relationship.</returns>
    public Selector SiblingOf(string sibling, string combinator) => new($"{sibling} {combinator} {Value}");

    /// <summary>
    /// Prefixes the current selector with the specified class selector.
    /// </summary>
    /// <param name="classSelector">The class selector to prefix to the current selector.</param>
    /// <returns>A new <see cref="Selector"/> instance with the specified class selector prefixed to it.</returns>
    public Selector WithClassPrefix(string classSelector) => new($"{classSelector} {Value}");

    /// <summary>
    /// Combines this selector with another selector using the specified combinator.
    /// </summary>
    /// <param name="otherSelector">The other selector to combine with this selector.</param>
    /// <param name="combinator">The combinator to use for combining the selectors. Defaults to an empty string.</param>
    /// <returns>A new <see cref="Selector"/> instance resulting from the combination.</returns>
    public Selector Combine(string otherSelector, string combinator = "") =>
        string.IsNullOrEmpty(combinator)
            ? new Selector($"{Value}{otherSelector}")
            : new Selector($"{Value}{combinator}{otherSelector}");

    /// <summary>
    /// Wraps the selector with a specified attribute selector.
    /// </summary>
    /// <param name="attribute">The attribute selector to prepend to the current selector.</param>
    /// <returns>A new <see cref="Selector"/> instance with the attribute selector included.</returns>
    public Selector WithAttribute(string attribute) => new($"{attribute} {Value}");

    /// <summary>
    /// Adds a :where() wrapper around the current selector to reduce specificity.
    /// </summary>
    /// <returns>A new <see cref="Selector"/> instance wrapped in a :where() function.</returns>
    public Selector InWhere() => new($":where({Value})");

    /// <summary>
    /// Creates a complex :where() clause with multiple selectors.
    /// </summary>
    /// <param name="selectors">An array of selectors to include within the :where() clause.</param>
    /// <returns>A new <see cref="Selector"/> instance with the combined :where() clause.</returns>
    public Selector InWhereWithMultiple(params string[] selectors) =>
        new($"{Value}:where({string.Join(", ", selectors)})");

    /// <summary>
    /// Adds a :is() wrapper around the current selector.
    /// </summary>
    /// <returns>A new <see cref="Selector"/> instance wrapped in a :is() pseudo-class.</returns>
    public Selector InIs() => new($":is({Value})");

    /// <summary>
    /// Wraps the current selector in a :not() pseudo-class for negation.
    /// </summary>
    /// <returns>A new <see cref="Selector"/> instance wrapped in a :not() pseudo-class.</returns>
    public Selector InNot() => new($":not({Value})");

    /// <summary>
    /// Adds a complex :not(:where()) wrapper for excluding a specific class in prose-style selectors.
    /// </summary>
    /// <param name="excludeClass">The class name to exclude from the selector.</param>
    /// <returns>A new <see cref="Selector"/> instance with the exclusion applied.</returns>
    public Selector WithNotProse(string excludeClass) =>
        new($":where({Value}):not(:where([class~=\"{excludeClass}\"],[class~=\"{excludeClass}\"] *))");

    /// <summary>
    /// Adds a prose-style element targeting using a parent class selector, an element, and an excluded class name.
    /// </summary>
    /// <param name="parentClass">The parent class selector, typically a general prose class (e.g., ".prose").</param>
    /// <param name="element">The element to target within the parent class (e.g., "p", "ul").</param>
    /// <param name="excludeClass">The class name to exclude from targeting, defaulting to "not-prose".</param>
    /// <returns>A new <see cref="Selector"/> instance representing the assembled selector with the specified parent class, element, and exclusion logic.</returns>
    public static Selector ProseElement(string parentClass, string element, string excludeClass = "not-prose") =>
        new($"{parentClass} :where({element}):not(:where([class~=\"{excludeClass}\"],[class~=\"{excludeClass}\"] *))");

    /// <inheritdoc />
    public override string ToString() => Value;
}