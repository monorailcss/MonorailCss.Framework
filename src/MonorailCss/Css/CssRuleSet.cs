using System.Collections.Immutable;
using System.Text;
using MonorailCss.Variants;

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
/// <param name="Importance">The importance of the rule. Will affect the ordering during output. Lower importance will be listed first.</param>
public record CssRuleSet(CssSelector Selector, CssDeclarationList DeclarationList, int Importance = 0)
{
    /// <summary>
    /// Adds two rule sets together.
    /// </summary>
    /// <param name="ruleSet1">The first rule set.</param>
    /// <param name="ruleSet2">The second rule set.</param>
    /// <returns>A new instance of the two rule sets combined.</returns>
    /// <exception cref="InvalidOperationException">Throws if the rule sets have different selectors.</exception>
    public static CssRuleSet operator +(CssRuleSet ruleSet1, CssRuleSet ruleSet2)
    {
        if (ruleSet1.Selector.Equals(ruleSet2.Selector) == false)
        {
            throw new InvalidOperationException("Cannot add ruleset with different selectors.");
        }

        return ruleSet1 with
        {
            DeclarationList = ruleSet1.DeclarationList + ruleSet2.DeclarationList,
            Importance = Math.Max(ruleSet1.Importance, ruleSet2.Importance),
        };
    }
}

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
            sb.Append(PseudoClass);
        }

        if (PseudoElement != default)
        {
            sb.Append(PseudoElement);
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    public virtual bool Equals(CssSelector? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Selector == other.Selector && PseudoClass == other.PseudoClass && PseudoElement == other.PseudoElement;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Selector, PseudoClass, PseudoElement);
    }

    /// <summary>
    /// Returns a new CSS selector from a string.
    /// </summary>
    /// <param name="selector">The selector.</param>
    /// <returns>A new CSS selector instance.</returns>
    public static implicit operator CssSelector(string selector) => new(selector);
}

/// <summary>
/// Represents a CSS declaration list.
/// </summary>
/// <param name="Property">The property.</param>
public abstract record BaseCssDeclaration(string Property)
{
    /// <summary>
    /// Returns a new CSS declaration from a string.
    /// </summary>
    /// <returns>The CSS declaration.</returns>
    public abstract string ToCssString();

    /// <summary>
    /// Converts a tuple to a CSS declaration.
    /// </summary>
    /// <param name="tuple">A two string tuple representing the property and value.</param>
    /// <returns>A CSS declaration object.</returns>
    public static implicit operator BaseCssDeclaration((string Property, string Value) tuple)
        => new CssDeclaration(tuple.Property, tuple.Value);
}

/// <summary>
/// Represents a CSS declaration.
/// </summary>
/// <param name="Property">The property.</param>
/// <param name="Value">The value.</param>
public record CssDeclaration(string Property, string Value) : BaseCssDeclaration(Property)
{
    /// <inheritdoc />
    public override string ToCssString()
    {
        return $"{Property}:{Value};";
    }
}

/// <summary>
/// Represents a CSS keyframe declaration.
/// </summary>
/// <param name="Property">The keyframe property name.</param>
/// <param name="DeclarationList">List of CSS properties this keyframe manipulates.</param>
public record CssKeyframeDeclaration(string Property, CssDeclarationList DeclarationList) : BaseCssDeclaration(Property)
{
    /// <inheritdoc />
    public override string ToCssString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{Property} {{");
        foreach (var declaration in DeclarationList)
        {
            sb.AppendLine($"  {declaration.ToCssString()}");
        }

        sb.Append('}');

        return sb.ToString();
    }
}

/// <summary>
/// Represents a CSS media rule declaration.
/// </summary>
/// <param name="Features">A list of media rules features.</param>
/// <param name="RuleSets">The defined rule sets for the media rule feature.</param>
public record CssMediaRule(ImmutableList<MediaQueryVariant> Features, ImmutableList<CssRuleSet> RuleSets);