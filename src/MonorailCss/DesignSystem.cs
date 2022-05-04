using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss;

/// <summary>
/// The configuration of a design system.
/// </summary>
public partial record DesignSystem
{
    private ImmutableDictionary<string, CssColor>? _flattenedColors;

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
    /// Gets a cached list of flattened colors.
    /// </summary>
    /// <returns>The flattened colors.</returns>
    internal ImmutableDictionary<string, CssColor> GetFlattenColors()
    {
        return _flattenedColors ??= Colors.Flatten();
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