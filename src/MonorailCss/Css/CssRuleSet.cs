using System.Collections.Immutable;
using System.Text;

namespace MonorailCss.Css;

/// <summary>
/// Represents a CSS style sheet.
/// </summary>
/// <param name="MediaRules">The list of media rules.</param>
public record CssStylesheet(ImmutableList<CssMediaRule> MediaRules);

/// <summary>
/// Represents a CSS rule set.
/// </summary>
/// <param name="Selector">The CSS selector.</param>
/// <param name="DeclarationList">The CSS declaration list.</param>
public record CssRuleSet(CssSelector Selector, CssDeclarationList DeclarationList);

/// <summary>
/// Represents a CSS selector.
/// </summary>
/// <param name="Selector">The selector.</param>
/// <param name="PseudoClass">The pseudo class, if it exists.</param>
/// <param name="PseudoElement">The pseudo element, if it exists.</param>
public record CssSelector(string Selector, string? PseudoClass = default, string? PseudoElement = default)
{
    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder(Selector);
        if (PseudoClass != default)
        {
            sb.Append($":{PseudoClass}");
        }

        if (PseudoElement != default)
        {
            sb.Append($"::{PseudoElement}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns a new CSS selector from a string.
    /// </summary>
    /// <param name="selector">The selector.</param>
    /// <returns>A new CSS selector instance.</returns>
    public static implicit operator CssSelector(string selector) => new CssSelector(selector);
}

/// <summary>
/// Represents a CSS declaration.
/// </summary>
/// <param name="Property">The property.</param>
/// <param name="Value">The value.</param>
public record CssDeclaration(string Property, string Value);

/// <summary>
/// Represents a CSS media rule declaration.
/// </summary>
/// <param name="Features">A list of media rules features.</param>
/// <param name="RuleSets">The defined rule sets for the media rule feature.</param>
public record CssMediaRule(ImmutableList<string> Features, ImmutableList<CssRuleSet> RuleSets);

/// <summary>
/// Represents the root media rule with no features.
/// </summary>
/// <param name="RuleSets">The rule sets.</param>
public record RootMediaRule(ImmutableList<CssRuleSet> RuleSets) : CssMediaRule(ImmutableList<string>.Empty, RuleSets);