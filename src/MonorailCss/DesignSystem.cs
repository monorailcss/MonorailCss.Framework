using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss;

/// <summary>
/// The configuration of a design system.
/// </summary>
public partial record DesignSystem
{
    /// <summary>
    /// Gets the screen design system.
    /// </summary>
    public ImmutableDictionary<string, string> Screens { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets the font size design system.
    /// </summary>
    public ImmutableDictionary<string, Typography> Typography { get; init; } =
        ImmutableDictionary<string, Typography>.Empty;

    /// <summary>
    /// Gets the font weight design system.
    /// </summary>
    public ImmutableDictionary<string, string> FontWeights { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets the color design system.
    /// </summary>
    public ImmutableDictionary<string, ImmutableDictionary<string, CssColor>> Colors { get; init; } =
        ImmutableDictionary<string, ImmutableDictionary<string, CssColor>>.Empty;

    /// <summary>
    /// Gets the spacing design system.
    /// </summary>
    public ImmutableDictionary<string, string> Spacing { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets the opacity design system.
    /// </summary>
    public ImmutableDictionary<string, string> Opacities { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets the settings for plugins.
    /// </summary>
    public ImmutableDictionary<string, ImmutableDictionary<string, object>> PluginSettings { get; init; } = ImmutableDictionary<string, ImmutableDictionary<string, object>>.Empty;

    /// <summary>
    /// Gets a plugin setting or returns a default value.
    /// </summary>
    /// <param name="pluginName">The plugin name. Recommended to use nameof(PluginName) for value.</param>
    /// <param name="setting">The plugin name. Recommended to use nameof(Setting) for value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <typeparam name="T">The expected type.</typeparam>
    /// <returns>The setting if set, otherwise the default value.</returns>
    public T GetPluginSetting<T>(string pluginName, string setting, T defaultValue)
    {
        if (!PluginSettings.TryGetValue(pluginName, out var settings))
        {
            return defaultValue;
        }

        if (!settings.TryGetValue(setting, out var settingValue))
        {
            return defaultValue;
        }

        if (typeof(T) != settingValue.GetType())
        {
            return defaultValue;
        }

        return (T)settingValue;
    }
}

/// <summary>
/// Represents a typographical design system entry.
/// </summary>
public record Typography
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Typography"/> class.
    /// </summary>
    /// <param name="fontSize">The font size.</param>
    /// <param name="lineHeight">The line height.</param>
    public Typography(string fontSize, string lineHeight)
    {
        this.FontSize = fontSize;
        this.LineHeight = lineHeight;
    }

    /// <summary>
    /// Gets the font size.
    /// </summary>
    public string FontSize { get; init; }

    /// <summary>
    /// Gets the line height.
    /// </summary>
    public string LineHeight { get; init; }
}