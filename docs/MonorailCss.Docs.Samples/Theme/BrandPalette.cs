using System.Collections.Immutable;
using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Theme;

/// <summary>
/// Adds a complete custom color palette ("brand") so utilities like
/// <c>bg-brand-500</c>, <c>text-brand-700</c>, and <c>ring-brand-300</c>
/// resolve to your design tokens.
/// </summary>
public static class BrandPalette
{
    /// <summary>
    /// Builds a framework with a "brand" palette covering the standard
    /// 50-950 scale. Shade keys must be strings; values can be any valid
    /// CSS color (hex, rgb(), oklch(), etc.).
    /// </summary>
    public static CssFramework Build()
    {
        var theme = new ThemeModel().AddColorPalette("brand", new Dictionary<string, string>
        {
            { "50",  "oklch(0.985 0.013 280)" },
            { "100", "oklch(0.961 0.027 280)" },
            { "200", "oklch(0.918 0.058 280)" },
            { "300", "oklch(0.852 0.108 280)" },
            { "400", "oklch(0.745 0.165 280)" },
            { "500", "oklch(0.638 0.207 280)" },
            { "600", "oklch(0.554 0.207 280)" },
            { "700", "oklch(0.464 0.182 280)" },
            { "800", "oklch(0.378 0.151 280)" },
            { "900", "oklch(0.298 0.116 280)" },
            { "950", "oklch(0.220 0.090 280)" },
        }.ToImmutableDictionary());

        return new CssFramework(new CssFrameworkSettings { Theme = theme });
    }
}
