using MonorailCss.Css;

namespace MonorailCss.Variants.BuiltIn;

/// <summary>
/// Variant for targeting specific elements within prose content.
/// Examples: prose-headings:text-lg, prose-a:underline, prose-h1:font-bold.
/// </summary>
internal sealed class ProseElementVariant : IVariant
{
    private readonly string _selector;
    private readonly int _weight;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProseElementVariant"/> class.
    /// Creates a prose element variant.
    /// </summary>
    /// <param name="element">The element name (e.g., "headings", "a", "h1").</param>
    /// <param name="selector">The CSS selector(s) to target.</param>
    /// <param name="weight">The variant weight for sorting.</param>
    public ProseElementVariant(string element, string selector, int weight)
    {
        _selector = selector;
        _weight = weight;
        Name = $"prose-{element}";
    }

    public string Name { get; }

    public int Weight => _weight;

    public VariantKind Kind => VariantKind.Static;

    public VariantConstraints Constraints => VariantConstraints.StyleRules;

    public bool CanHandle(VariantToken token)
    {
        return token.Name == Name && !token.IsArbitrary;
    }

    public bool TryApply(AppliedSelector selector, VariantToken token, out AppliedSelector result)
    {
        if (!CanHandle(token))
        {
            result = selector;
            return false;
        }

        // Build the prose element selector with :where() and not-prose exclusion
        // Pattern: .class :where(element):not(:where([class~="not-prose"],[class~="not-prose"] *))
        // Append the element targeting to the existing selector instead of replacing it
        var elementTargeting = $" :where({_selector}):not(:where([class~=\"not-prose\"],[class~=\"not-prose\"] *))";
        var newSelector = new Selector(selector.Selector.Value + elementTargeting);

        // Preserve the existing selector and append prose element targeting
        result = new AppliedSelector(newSelector, selector.Wrappers);
        return true;
    }
}