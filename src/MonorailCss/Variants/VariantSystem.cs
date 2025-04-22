using System.Collections.Immutable;
using MonorailCss.Plugins;

namespace MonorailCss.Variants;

/// <summary>
/// System for defining variants for a CSS framework.
/// </summary>
public class VariantSystem
{
    private readonly IVariantPluginProvider[] _plugins;

    /// <summary>
    /// Gets the configured variants.
    /// </summary>
    public ImmutableDictionary<string, IVariant> Variants { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VariantSystem"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    /// <param name="plugins">All the configured plugins.</param>
    public VariantSystem(DesignSystem designSystem, IVariantPluginProvider[] plugins)
    {
        _plugins = plugins;
        Variants = GetDefaultVariantSystem(designSystem);
    }

    private ImmutableDictionary<string, IVariant> GetDefaultVariantSystem(DesignSystem designSystem)
    {
        var variants = new Dictionary<string, IVariant>();

        // Positional
        AddPseudoClass("first", ":first-child");
        AddPseudoClass("last", ":last-child");
        AddPseudoClass("only", ":only-child");
        AddPseudoClass("odd", ":nth-child(odd)");
        AddPseudoClass("even", ":nth-child(even)");
        AddPseudoClass("first-of-type");
        AddPseudoClass("last-of-type");
        AddPseudoClass("only-of-type");
        AddPseudoClass("nth-1", ":nth-child(1)");
        AddPseudoClass("nth-2", ":nth-child(2)");
        AddPseudoClass("nth-3", ":nth-child(3)");
        AddPseudoClass("nth-4", ":nth-child(4)");
        AddPseudoClass("nth-5", ":nth-child(5)");
        AddPseudoClass("nth-last-1", ":nth-last-child(1)");
        AddPseudoClass("nth-last-2", ":nth-last-child(2)");
        AddPseudoClass("nth-last-3", ":nth-last-child(3)");

        // ARIA states
        AddPseudoClass("aria-busy", "[aria-busy=\"true\"]");
        AddPseudoClass("aria-checked", "[aria-checked=\"true\"]");
        AddPseudoClass("aria-disabled", "[aria-disabled=\"true\"]");
        AddPseudoClass("aria-expanded", "[aria-expanded=\"true\"]");
        AddPseudoClass("aria-hidden", "[aria-hidden=\"true\"]");
        AddPseudoClass("aria-pressed", "[aria-pressed=\"true\"]");
        AddPseudoClass("aria-readonly", "[aria-readonly=\"true\"]");
        AddPseudoClass("aria-required", "[aria-required=\"true\"]");
        AddPseudoClass("aria-selected", "[aria-selected=\"true\"]");

        // State
        AddPseudoClass("target");
        AddPseudoClass("open", "[open]");
        AddPseudoClass("visited");
        AddPseudoClass("current", ":is(:current)");

        // Forms
        AddPseudoClass("default");
        AddPseudoClass("checked");
        AddPseudoClass("indeterminate");
        AddPseudoClass("placeholder-shown");
        AddPseudoClass("autofill");
        AddPseudoClass("required");
        AddPseudoClass("valid");
        AddPseudoClass("invalid");
        AddPseudoClass("in-range");
        AddPseudoClass("out-of-range");
        AddPseudoClass("read-only");

        // Content
        AddPseudoClass("empty");

        // Interactive
        AddPseudoClass("focus-within");
        AddPseudoClass("hover");
        AddPseudoClass("focus");
        AddPseudoClass("focus-visible");
        AddPseudoClass("active");
        AddPseudoClass("disabled");

        // Theme variants
        variants.Add("dark", new SelectorVariant(".dark"));
        variants.Add("print", new MediaQueryVariant("print"));

        variants.Add("rtl", new SelectorVariant("[dir='rtl']"));
        variants.Add("ltr", new SelectorVariant("[dir='ltr']"));

        variants.Add("forced-colors", new MediaQueryVariant("(forced-colors: active)"));
        variants.Add("not-forced-colors", new MediaQueryVariant("(forced-colors: none)"));
        variants.Add("inverted-colors", new MediaQueryVariant("(inverted-colors: inverted)"));
        variants.Add("pointer-fine", new MediaQueryVariant("(pointer: fine)"));
        variants.Add("pointer-coarse", new MediaQueryVariant("(pointer: coarse)"));
        variants.Add("any-pointer-fine", new MediaQueryVariant("(any-pointer: fine)"));
        variants.Add("any-pointer-coarse", new MediaQueryVariant("(any-pointer: coarse)"));

        // Screen size variants
        foreach (var (key, size) in designSystem.Screens)
        {
            // try and extract the size and use this as a priority. we want the screens
            // with the largest sizes listed after the smaller ones so that if someone specifies
            // something like md:bg-green-200 sm:bg-blue-200 bg-red-200 the media variants
            // are ordered so that the largest screen overrides the smaller screens as well as the
            // selector with no variant.
            var variant = int.TryParse(size.Replace("px", string.Empty), out var sizeValue)
                ? new MediaQueryVariant($"(min-width:{size})", sizeValue)
                : new MediaQueryVariant($"(min-width:{size})");

            variants.Add(key, variant);
        }

        foreach (var (key, size) in designSystem.Screens)
        {
            // Min-width container queries
            var minContainerVariant = int.TryParse(size.Replace("px", string.Empty), out var sizeValue)
                ? new MediaQueryVariant($"(container: min-width {size})", sizeValue)
                : new MediaQueryVariant($"(container: min-width {size})");

            variants.Add($"@min-{key}", minContainerVariant);

            // Max-width container queries
            var maxContainerVariant = int.TryParse(size.Replace("px", string.Empty), out var maxSizeValue)
                ? new MediaQueryVariant($"(container: max-width {size})", maxSizeValue)
                : new MediaQueryVariant($"(container: max-width {size})");

            variants.Add($"@max-{key}", maxContainerVariant);
        }

        // Pseudo-elements
        variants.Add("placeholder", new PseudoElementVariant("::placeholder"));
        variants.Add("before", new PseudoElementVariant("::before"));
        variants.Add("after", new PseudoElementVariant("::after"));
        variants.Add("selection", new PseudoElementVariant("::selection"));
        variants.Add("file", new PseudoElementVariant("::file-selector-button"));
        variants.Add("marker", new PseudoElementVariant("::marker"));
        variants.Add("has-checked", new SelectorVariant(":has(:checked)"));
        variants.Add("has-hover", new SelectorVariant(":has(:hover)"));
        variants.Add("has-focus", new SelectorVariant(":has(:focus)"));
        variants.Add("has-active", new SelectorVariant(":has(:active)"));
        variants.Add("has-invalid", new SelectorVariant(":has(:invalid)"));

        foreach (var plugin in _plugins)
        {
            foreach (var (modifier, variant) in plugin.GetVariants())
            {
                variants.Add(modifier, variant);
            }
        }

        return variants.ToImmutableDictionary();

        void AddPseudoClass(string name, string? css = null)
        {
            var pseudoClass = css ?? $":{name}";
            variants.Add(name, new PseudoClassVariant(pseudoClass));
            variants.Add($"peer-{name}", new ConditionalVariant(name, $"&:is(:where(.peer){pseudoClass} ~ *)"));
            variants.Add($"group-{name}", new ConditionalVariant(name, $"&:is(:where(.group){pseudoClass} *)"));
        }
    }
}