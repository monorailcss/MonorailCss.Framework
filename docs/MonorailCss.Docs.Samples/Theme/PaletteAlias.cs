using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Theme;

/// <summary>
/// Creates a semantic alias for an existing palette. <c>--color-primary-*</c>
/// becomes a thin pointer that resolves to <c>--color-sky-*</c>, so
/// <c>bg-primary-500</c> renders the same color as <c>bg-sky-500</c>
/// but the design intent ("primary brand color") is captured in the class
/// name.
/// </summary>
public static class PaletteAlias
{
    /// <summary>
    /// Maps the built-in <c>sky</c> palette to a new <c>primary</c> alias
    /// across every standard shade (50-950). Swap the source palette later
    /// without touching any markup.
    /// </summary>
    public static CssFramework Build()
    {
        var theme = new ThemeModel().MapColorPalette("sky", "primary");

        return new CssFramework(new CssFrameworkSettings { Theme = theme });
    }
}
