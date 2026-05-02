using System.Collections.Immutable;
using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Theme;

/// <summary>
/// Starts from a completely empty theme and rebuilds only the design
/// tokens you care about. Useful when bundling MonorailCss into a brand
/// system that doesn't want to ship the default Tailwind palette at all.
/// </summary>
public static class EmptyTheme
{
    /// <summary>
    /// Creates a framework whose theme has no defaults &#8212; only the
    /// single <c>brand</c> palette declared here is available. Utilities
    /// that reference missing tokens (e.g. <c>bg-red-500</c>) will simply
    /// not be emitted.
    /// </summary>
    public static CssFramework Build()
    {
        var theme = ThemeModel.CreateEmpty()
            .AddColorPalette("brand", new Dictionary<string, string>
            {
                { "500", "#3b82f6" },
                { "700", "#1d4ed8" },
            }.ToImmutableDictionary());

        return new CssFramework(new CssFrameworkSettings { Theme = theme });
    }
}
