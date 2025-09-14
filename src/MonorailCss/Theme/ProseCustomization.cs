using System.Collections.Immutable;

namespace MonorailCss.Theme;

/// <summary>
/// Represents customization settings for the prose utility, allowing users to add new elements
/// or override settings for existing elements.
/// </summary>
public record ProseCustomization
{
    /// <summary>
    /// Gets function to generate customizations for prose elements.
    /// The function receives the theme and returns rules for different modifiers.
    /// Keys are modifier names: "DEFAULT", "base", "sm", "lg", "xl", "2xl", "invert"
    /// For static customizations that don't need theme values, the theme parameter can be ignored.
    /// </summary>
    public required Func<Theme, ImmutableDictionary<string, ProseElementRules>> Customization { get; init; }

    /// <summary>
    /// Retrieves the prose element rules for a given theme by applying customization logic.
    /// </summary>
    /// <param name="theme">The theme instance to use for retrieving the prose element rules.</param>
    /// <returns>A dictionary containing the configured prose element rules for the specified theme.</returns>
    public ImmutableDictionary<string, ProseElementRules> GetRules(Theme theme)
    {
        return Customization(theme);
    }
}

/// <summary>
/// Represents CSS rules for prose elements within a specific modifier context.
/// </summary>
public record ProseElementRules
{
    /// <summary>
    /// Gets list of element selectors and their CSS declarations.
    /// </summary>
    public ImmutableList<ProseElementRule> Rules { get; init; } = ImmutableList<ProseElementRule>.Empty;

    /// <summary>
    /// Combines the current instance of <see cref="ProseElementRules"/> with another instance,
    /// merging their rules. Rules with the same selector are merged, with declarations in the
    /// other instance taking precedence.
    /// </summary>
    /// <param name="other">The other <see cref="ProseElementRules"/> instance to merge with.</param>
    /// <returns>A new instance of <see cref="ProseElementRules"/> containing the merged rules.</returns>
    public ProseElementRules MergeWith(ProseElementRules other)
    {
        var mergedRules = new Dictionary<string, ProseElementRule>();

        // Add all rules from this instance
        foreach (var rule in Rules)
        {
            mergedRules[rule.Selector] = rule;
        }

        // Override or add rules from other
        foreach (var rule in other.Rules)
        {
            if (mergedRules.TryGetValue(rule.Selector, out var existingRule))
            {
                // Merge declarations for the same selector
                mergedRules[rule.Selector] = existingRule.MergeWith(rule);
            }
            else
            {
                mergedRules[rule.Selector] = rule;
            }
        }

        return new ProseElementRules
        {
            Rules = mergedRules.Values.ToImmutableList(),
        };
    }
}

/// <summary>
/// Represents a CSS rule for a specific prose element.
/// </summary>
public record ProseElementRule
{
    /// <summary>
    /// Gets the CSS selector for the element (e.g., "a", "blockquote", "pre > code").
    /// </summary>
    public required string Selector { get; init; }

    /// <summary>
    /// Gets cSS declarations for this element.
    /// </summary>
    public ImmutableList<ProseDeclaration> Declarations { get; init; } = ImmutableList<ProseDeclaration>.Empty;

    /// <summary>
    /// Gets a value indicating whether whether to use :where() wrapper for this rule.
    /// Default is true for consistency with prose utility.
    /// </summary>
    public bool UseWhereWrapper { get; init; } = true;

    /// <summary>
    /// Gets class to exclude (e.g., "not-prose"). Null means no exclusion.
    /// </summary>
    public string? ExcludeClass { get; init; } = "not-prose";

    /// <summary>
    /// Merges the current set of prose element rules with another set, combining or overriding rules as needed.
    /// </summary>
    /// <param name="other">The other set of prose element rules to merge with.</param>
    /// <returns>A new instance of <see cref="ProseElementRules"/> containing the merged rules from both sets.</returns>
    public ProseElementRule MergeWith(ProseElementRule other)
    {
        var mergedDeclarations = new Dictionary<string, ProseDeclaration>();

        // Add all declarations from this rule
        foreach (var decl in Declarations)
        {
            mergedDeclarations[decl.Property] = decl;
        }

        // Override or add declarations from other
        foreach (var decl in other.Declarations)
        {
            mergedDeclarations[decl.Property] = decl;
        }

        return new ProseElementRule
        {
            Selector = Selector,
            Declarations = mergedDeclarations.Values.ToImmutableList(),
            UseWhereWrapper = other.UseWhereWrapper, // Use the override's setting
            ExcludeClass = other.ExcludeClass ?? ExcludeClass, // Use override if not null
        };
    }
}

/// <summary>
/// Represents a CSS declaration for prose customization.
/// </summary>
public record ProseDeclaration
{
    /// <summary>
    /// Gets the CSS property name (e.g., "font-weight", "border-left-width").
    /// </summary>
    public required string Property { get; init; }

    /// <summary>
    /// Gets the CSS value for the property.
    /// Can be a static value or a function that generates a value based on the theme.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// Gets a value indicating whether whether this declaration is marked as important.
    /// </summary>
    public bool Important { get; init; }
}