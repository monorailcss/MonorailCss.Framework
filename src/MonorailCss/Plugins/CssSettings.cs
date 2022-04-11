using MonorailCss.Css;

namespace MonorailCss.Plugins;

/// <summary>
/// Configuration of a CSS settings.
/// </summary>
public record CssSettings
{
    /// <summary>
    /// Gets the declarations for the root element.
    /// </summary>
    public CssDeclarationList Css { get; init; } = new();

    /// <summary>
    /// Gets the child rules for the element.
    /// </summary>
    public CssRuleSetList ChildRules { get; init; } = new();

    /// <summary>
    /// Combines two settings together. The right operator will override.
    /// </summary>
    /// <param name="settings1">The first settings.</param>
    /// <param name="settings2">The second settings.</param>
    /// <returns>A new settings instance.</returns>
    public static CssSettings operator +(CssSettings settings1, CssSettings settings2)
    {
        return settings1 with
        {
            ChildRules = settings1.ChildRules + settings2.ChildRules,
            Css = settings1.Css + settings2.Css,
        };
    }
}