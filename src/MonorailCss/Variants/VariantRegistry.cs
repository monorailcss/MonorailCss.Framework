using System.Diagnostics.CodeAnalysis;
using MonorailCss.Css;
using MonorailCss.Variants.BuiltIn;

namespace MonorailCss.Variants;

/// <summary>
/// Registry for managing and applying variants with canonical ordering.
/// </summary>
internal sealed class VariantRegistry
{
    private readonly Dictionary<string, IVariant> _variants = new();
    private readonly List<IVariant> _orderedVariants = [];

    /// <summary>
    /// Registers a variant in the registry.
    /// </summary>
    /// <param name="variant">The variant to register.</param>
    /// <param name="overwrite">Whether to overwrite an existing variant with the same name.</param>
    public void Register(IVariant variant, bool overwrite = false)
    {
        if (variant == null)
        {
            throw new ArgumentNullException(nameof(variant));
        }

        if (_variants.ContainsKey(variant.Name))
        {
            if (!overwrite)
            {
                throw new InvalidOperationException($"Variant '{variant.Name}' is already registered. Set overwrite=true to replace it.");
            }

            // Remove old variant from ordered list
            _orderedVariants.RemoveAll(v => v.Name == variant.Name);
        }

        _variants[variant.Name] = variant;
        _orderedVariants.Add(variant);

        // Keep ordered list sorted by weight
        _orderedVariants.Sort((a, b) => a.Weight.CompareTo(b.Weight));
    }

    /// <summary>
    /// Attempts to get a variant by name.
    /// </summary>
    public bool TryGet(string name, [NotNullWhen(true)] out IVariant? variant)
    {
        return _variants.TryGetValue(name, out variant);
    }

    /// <summary>
    /// Gets all registered variants in weight order.
    /// </summary>
    public IReadOnlyList<IVariant> GetAll() => _orderedVariants.AsReadOnly();

    /// <summary>
    /// Applies a sequence of variant tokens to create a final selector.
    /// </summary>
    /// <param name="baseClass">The base class name (already escaped).</param>
    /// <param name="variants">The variant tokens to apply.</param>
    /// <returns>The final applied selector with all transformations.</returns>
    public AppliedSelector ApplyVariants(string baseClass, IReadOnlyList<VariantToken> variants)
    {
        var result = AppliedSelector.FromClass(baseClass);

        if (variants.Count == 0)
        {
            return result;
        }

        // Apply variants in forward order to maintain source order
        // This ensures hover:focus generates :hover:focus, not :focus:hover
        for (var i = 0; i < variants.Count; i++)
        {
            var token = variants[i];
            var applied = false;

            // Try to find a variant that can handle this token
            foreach (var variant in _orderedVariants)
            {
                if (variant.CanHandle(token))
                {
                    if (variant.TryApply(result, token, out var newResult))
                    {
                        result = newResult;
                        applied = true;
                        break;
                    }
                }
            }

            if (!applied)
            {
                // Unknown variant - could log warning or throw
                // For now, we'll just skip it
            }
        }

        return result;
    }

    /// <summary>
    /// Calculates the combined weight for a sequence of variants.
    /// Used for sorting classes with multiple variants.
    /// </summary>
    public int[] GetVariantWeights(IReadOnlyList<VariantToken> variants)
    {
        if (variants.Count == 0)
        {
            return [];
        }

        var weights = new int[variants.Count];

        for (var i = 0; i < variants.Count; i++)
        {
            var token = variants[i];
            var weight = int.MaxValue; // Default for unknown variants

            // Try direct lookup first for known variant names
            if (_variants.TryGetValue(token.Name, out var variant))
            {
                weight = variant.Weight;
            }
            else
            {
                // Fall back to checking if any variant can handle this token
                // (needed for arbitrary variants and parameterized variants)
                foreach (var v in _orderedVariants)
                {
                    if (v.CanHandle(token))
                    {
                        weight = v.Weight;
                        break;
                    }
                }
            }

            weights[i] = weight;
        }

        return weights;
    }

    /// <summary>
    /// Registers all built-in variants with their canonical weights.
    /// </summary>
    public void RegisterBuiltInVariants(Theme.Theme theme)
    {
        Register(new DirectionalityVariant("rtl", 100));
        Register(new DirectionalityVariant("ltr", 110));
        Register(new MotionVariant("motion-safe", 130));
        Register(new MotionVariant("motion-reduce", 140));

        Register(new GroupVariant(200));
        Register(new PeerVariant(250));

        // Prose element variants (before pseudo-class variants)
        Register(new ProseElementVariant("headings", "h1, h2, h3, h4, h5, h6, th", 285));
        Register(new ProseElementVariant("h1", "h1", 286));
        Register(new ProseElementVariant("h2", "h2", 286));
        Register(new ProseElementVariant("h3", "h3", 286));
        Register(new ProseElementVariant("h4", "h4", 286));
        Register(new ProseElementVariant("h5", "h5", 286));
        Register(new ProseElementVariant("h6", "h6", 286));
        Register(new ProseElementVariant("p", "p", 287));
        Register(new ProseElementVariant("a", "a", 287));
        Register(new ProseElementVariant("blockquote", "blockquote", 287));
        Register(new ProseElementVariant("figure", "figure", 287));
        Register(new ProseElementVariant("figcaption", "figcaption", 287));
        Register(new ProseElementVariant("strong", "strong", 287));
        Register(new ProseElementVariant("em", "em", 287));
        Register(new ProseElementVariant("kbd", "kbd", 287));
        Register(new ProseElementVariant("code", "code", 287));
        Register(new ProseElementVariant("pre", "pre", 287));
        Register(new ProseElementVariant("ol", "ol", 288));
        Register(new ProseElementVariant("ul", "ul", 288));
        Register(new ProseElementVariant("li", "li", 288));
        Register(new ProseElementVariant("table", "table", 289));
        Register(new ProseElementVariant("thead", "thead", 289));
        Register(new ProseElementVariant("tr", "tr", 289));
        Register(new ProseElementVariant("th", "th", 289));
        Register(new ProseElementVariant("td", "td", 289));
        Register(new ProseElementVariant("img", "img", 290));
        Register(new ProseElementVariant("video", "video", 290));
        Register(new ProseElementVariant("hr", "hr", 290));
        Register(new ProseElementVariant("lead", "[class~=\"lead\"]", 291));

        Register(new PseudoClassVariant("hover", ":hover", 300));
        Register(new PseudoClassVariant("focus", ":focus", 310));
        Register(new PseudoClassVariant("focus-visible", ":focus-visible", 320));
        Register(new PseudoClassVariant("active", ":active", 330));
        Register(new PseudoClassVariant("visited", ":visited", 340));
        Register(new PseudoClassVariant("disabled", ":disabled", 350));
        Register(new PseudoClassVariant("enabled", ":enabled", 360));
        Register(new PseudoClassVariant("checked", ":checked", 370));
        Register(new PseudoClassVariant("indeterminate", ":indeterminate", 380));

        // Functional attribute variants (data-[], aria-[], has-[], etc.) come before structural pseudo-classes
        Register(new FunctionalAttributeVariant("data", 385));
        Register(new FunctionalAttributeVariant("aria", 386));
        Register(new FunctionalAttributeVariant("has", 387));
        Register(new FunctionalAttributeVariant("where", 388));
        Register(new FunctionalAttributeVariant("is", 389));
        Register(new FunctionalAttributeVariant("not", 390));

        // Arbitrary variants for custom selectors
        Register(new ArbitraryVariant(395));

        // Supports variant for @supports feature queries
        Register(new SupportsVariant(400));

        Register(new PseudoClassVariant("first", ":first-child", 410));
        Register(new PseudoClassVariant("last", ":last-child", 420));
        Register(new PseudoClassVariant("odd", ":nth-child(odd)", 430));
        Register(new PseudoClassVariant("even", ":nth-child(even)", 440));
        Register(new PseudoClassVariant("first-of-type", ":first-of-type", 450));
        Register(new PseudoClassVariant("last-of-type", ":last-of-type", 460));
        Register(new PseudoClassVariant("only", ":only-child", 470));
        Register(new PseudoClassVariant("empty", ":empty", 480));

        Register(new PseudoElementVariant("before", "::before", 500));
        Register(new PseudoElementVariant("after", "::after", 510));
        Register(new PseudoElementVariant("first-line", "::first-line", 520));
        Register(new PseudoElementVariant("first-letter", "::first-letter", 530));
        Register(new PseudoElementVariant("marker", "::marker", 540));
        Register(new PseudoElementVariant("selection", "::selection", 550));
        Register(new PseudoElementVariant("placeholder", "::placeholder", 560));

        Register(new BreakpointVariant("sm", "(min-width: 640px)", 600));
        Register(new BreakpointVariant("md", "(min-width: 768px)", 610));
        Register(new BreakpointVariant("lg", "(min-width: 1024px)", 620));
        Register(new BreakpointVariant("xl", "(min-width: 1280px)", 630));
        Register(new BreakpointVariant("2xl", "(min-width: 1536px)", 640));

        Register(new PrintVariant(650));
        Register(new ContrastVariant("contrast-more", 660));
        Register(new ContrastVariant("contrast-less", 670));

        Register(new WhereClauseDarkVariant(680));

        // Static container query variants (@sm, @md, @lg, etc.)
        Register(new ContainerQueryVariant("@3xs", "3xs", ContainerQueryType.Min, theme, 700));
        Register(new ContainerQueryVariant("@2xs", "2xs", ContainerQueryType.Min, theme, 705));
        Register(new ContainerQueryVariant("@xs", "xs", ContainerQueryType.Min, theme, 710));
        Register(new ContainerQueryVariant("@sm", "sm", ContainerQueryType.Min, theme, 715));
        Register(new ContainerQueryVariant("@md", "md", ContainerQueryType.Min, theme, 720));
        Register(new ContainerQueryVariant("@lg", "lg", ContainerQueryType.Min, theme, 725));
        Register(new ContainerQueryVariant("@xl", "xl", ContainerQueryType.Min, theme, 730));
        Register(new ContainerQueryVariant("@2xl", "2xl", ContainerQueryType.Min, theme, 735));
        Register(new ContainerQueryVariant("@3xl", "3xl", ContainerQueryType.Min, theme, 740));
        Register(new ContainerQueryVariant("@4xl", "4xl", ContainerQueryType.Min, theme, 745));
        Register(new ContainerQueryVariant("@5xl", "5xl", ContainerQueryType.Min, theme, 750));
        Register(new ContainerQueryVariant("@6xl", "6xl", ContainerQueryType.Min, theme, 755));
        Register(new ContainerQueryVariant("@7xl", "7xl", ContainerQueryType.Min, theme, 760));

        // Functional container query variants (@min-*, @max-*)
        Register(new ContainerQueryVariant("@min", string.Empty, ContainerQueryType.MinFunctional, theme, 770));
        Register(new ContainerQueryVariant("@max", string.Empty, ContainerQueryType.MaxFunctional, theme, 780));
    }
}