using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Theme;

/// <summary>
/// Registers a custom font family so utilities like <c>font-display</c>
/// resolve to your chosen font stack. The first argument is the utility
/// suffix; the second is the CSS font-family value (commas, quotes, and
/// fallbacks all welcome).
/// </summary>
public static class FontFamily
{
    /// <summary>
    /// Adds a <c>display</c> family backed by Inter, plus a <c>mono</c>
    /// family for code blocks.
    /// </summary>
    public static CssFramework Build()
    {
        var theme = new ThemeModel()
            .AddFontFamily("display", "'Inter', system-ui, sans-serif")
            .AddFontFamily("mono", "'JetBrains Mono', ui-monospace, monospace");

        return new CssFramework(new CssFrameworkSettings { Theme = theme });
    }
}
