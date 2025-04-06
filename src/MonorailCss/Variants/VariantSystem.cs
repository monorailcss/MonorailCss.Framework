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

        void AddPseudoClass(string name, string? css = null) =>
            variants.Add(name, new PseudoClassVariant(css ?? $":{name}"));

        // Positional
        AddPseudoClass("first", ":first-child");
        AddPseudoClass("last", ":last-child");
        AddPseudoClass("only", ":only-child");
        AddPseudoClass("odd", ":nth-child(odd)");
        AddPseudoClass("even", ":nth-child(even)");
        AddPseudoClass("first-of-type");
        AddPseudoClass("last-of-type");
        AddPseudoClass("only-of-type");

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

        variants.Add("dark", new SelectorVariant(".dark"));
        variants.Add("print", new MediaQueryVariant("print"));

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

        variants.Add("placeholder", new PseudoElementVariant("::placeholder"));

        foreach (var plugin in _plugins)
        {
            foreach (var (modifier, variant) in plugin.GetVariants())
            {
                variants.Add(modifier, variant);
            }
        }

        return variants.ToImmutableDictionary();
    }
}