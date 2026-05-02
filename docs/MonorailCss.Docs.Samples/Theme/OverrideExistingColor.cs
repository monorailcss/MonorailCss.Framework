using ThemeModel = MonorailCss.Theme.Theme;

namespace MonorailCss.Docs.Samples.Theme;

/// <summary>
/// Replaces individual shades inside a built-in palette without touching
/// the rest. The theme stores every color as a CSS custom property keyed
/// <c>--color-{name}-{shade}</c>, so any single value can be swapped with
/// <see cref="ThemeModel.Add(string, string)"/>.
/// </summary>
public static class OverrideExistingColor
{
    /// <summary>
    /// Overrides <c>blue-500</c> and <c>blue-600</c> with custom values
    /// while leaving the rest of the blue scale (and every other palette)
    /// untouched.
    /// </summary>
    public static CssFramework Build()
    {
        var theme = new ThemeModel()
            .Add("--color-blue-500", "#1d4ed8")
            .Add("--color-blue-600", "#1e40af");

        return new CssFramework(new CssFrameworkSettings { Theme = theme });
    }
}
