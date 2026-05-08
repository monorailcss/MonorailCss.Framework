using System.Collections.Immutable;
using MonorailCss.Parser.Custom;
using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Combined;

/// <summary>
/// End-to-end example that wires every customization knob in one place:
/// a brand color palette, a font family, component classes via
/// <c>Applies</c>, scrollbar utilities, and matching pseudo-element
/// variants.
/// </summary>
public static class FullSetup
{
    /// <summary>
    /// Builds a fully-configured framework. Mirrors what a real
    /// application's startup wiring tends to look like.
    /// </summary>
    public static CssFramework Build()
    {
        var theme = new ThemeModel()
            .AddColorPalette("brand", new Dictionary<string, string>
            {
                { "50",  "oklch(0.985 0.013 280)" },
                { "500", "oklch(0.638 0.207 280)" },
                { "700", "oklch(0.464 0.182 280)" },
                { "950", "oklch(0.220 0.090 280)" },
            }.ToImmutableDictionary())
            .MapColorPalette("brand", "primary")
            .AddFontFamily("display", "'Inter', system-ui, sans-serif");

        var applies = new Dictionary<string, string>
        {
            { ".btn", "px-4 py-2 rounded-lg font-semibold font-display" },
            { ".btn-primary", "bg-primary-500 text-white hover:bg-primary-700" },
            { ".scroll-area", "scrollbar-thin scrollbar-thumb:bg-primary-500" },
        }.ToImmutableDictionary();

        return new CssFramework(new CssFrameworkSettings
        {
            Theme = theme,
            Applies = applies,
            CustomUtilities = new UtilityDefinition
            {
                Pattern = "scrollbar-thin",
                Declarations = new CssDeclaration("scrollbar-width", "thin"),
            },
            CustomVariants = new CustomVariantDefinition
            {
                Name = "scrollbar-thumb",
                Selector = "&::-webkit-scrollbar-thumb",
                Weight = 491,
            },
            IncludePreflight = true,
        });
    }
}
