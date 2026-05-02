using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Settings;

/// <summary>
/// Disables the built-in preflight (reset) styles. Turn this off when
/// MonorailCss is generating a small slice of CSS that ships alongside
/// another framework's reset (Bootstrap, an existing design system, a
/// CMS theme), so you don't double-reset margins, headings, and form
/// controls.
/// </summary>
public static class Preflight
{
    /// <summary>
    /// Builds a framework that skips emitting preflight base styles.
    /// </summary>
    public static CssFramework Build()
    {
        return new CssFramework(new CssFrameworkSettings
        {
            Theme = new ThemeModel(),
            IncludePreflight = false,
        });
    }
}
