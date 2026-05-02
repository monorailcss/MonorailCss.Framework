using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Settings;

/// <summary>
/// Forces every generated declaration to emit with <c>!important</c>.
/// Reach for this only when MonorailCss is being layered on top of an
/// existing stylesheet that already wins specificity wars; in greenfield
/// projects the default (false) is almost always what you want.
/// </summary>
public static class Important
{
    /// <summary>
    /// Builds a framework that emits <c>!important</c> on every rule.
    /// </summary>
    public static CssFramework Build()
    {
        return new CssFramework(new CssFrameworkSettings
        {
            Theme = new ThemeModel(),
            Important = true,
        });
    }
}
