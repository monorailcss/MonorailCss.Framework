using System.Collections.Immutable;
using MonorailCss.Css;
using MonorailCss.Parser;

namespace MonorailCss.Plugins.Borders;

/// <summary>
/// The border-color plugin.
/// </summary>
public class BorderColor : IUtilityNamespacePlugin
{
    private readonly ImmutableDictionary<string, CssColor> _flattenedColors;
    private readonly ImmutableDictionary<string, string> _opacity;
    private ImmutableDictionary<string, CssColor>? _completeColors;

    private static readonly ImmutableDictionary<string, string[]> NamespaceToProperties = new Dictionary<string, string[]>
    {
        { "border", [CssProperties.BorderColor] },
        { "border-x", [CssProperties.BorderLeftColor, CssProperties.BorderRightColor] },
        { "border-y", [CssProperties.BorderTopColor, CssProperties.BorderBottomColor] },
        { "border-r", [CssProperties.BorderRightColor] },
        { "border-t", [CssProperties.BorderTopColor] },
        { "border-b", [CssProperties.BorderBottomColor] },
        { "border-l", [CssProperties.BorderLeftColor] },
    }.ToImmutableDictionary();

    /// <summary>
    /// Initializes a new instance of the <see cref="BorderColor"/> class.
    /// </summary>
    /// <param name="designSystem">The design system.</param>
    public BorderColor(DesignSystem designSystem)
    {
        _flattenedColors = designSystem.GetFlattenColors();
        _opacity = designSystem.Opacities;
    }

    /// <inheritdoc />
    public ImmutableArray<string> Namespaces => [..NamespaceToProperties.Keys];

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> Process(IParsedClassNameSyntax syntax)
    {
        switch (syntax)
        {
            case ArbitraryValueSyntax arbitraryValueSyntax when NamespaceToProperties.ContainsKey(arbitraryValueSyntax.Namespace):
                {
                    // Only process arbitrary values that are valid colors, not size values
                    if (!IsValidColorValue(arbitraryValueSyntax.ArbitraryValue))
                    {
                        yield break;
                    }

                    var color = new CssColor(arbitraryValueSyntax.ArbitraryValue);
                    var properties = NamespaceToProperties[arbitraryValueSyntax.Namespace];
                    var declarations = GetDeclarations(color, null, properties);
                    yield return new CssRuleSet(arbitraryValueSyntax.OriginalSyntax, declarations);
                    break;
                }

            case NamespaceSyntax namespaceSyntax when NamespaceToProperties.ContainsKey(namespaceSyntax.Namespace):
                {
                    var suffix = namespaceSyntax.Suffix ?? "DEFAULT";
                    var (colorValue, opacityValue) = ColorParser.SplitColor(suffix);

                    if (!AllColors().TryGetValue(colorValue, out var color))
                    {
                        yield break;
                    }

                    var properties = NamespaceToProperties[namespaceSyntax.Namespace];
                    var declarations = GetDeclarations(color, opacityValue, properties);
                    yield return new CssRuleSet(namespaceSyntax.OriginalSyntax, declarations);
                    break;
                }

            default:
                yield break;
        }
    }

    private CssDeclarationList GetDeclarations(CssColor color, string? opacityValue, string[] properties)
    {
        var declarations = new CssDeclarationList();

        string colorValue;
        if (opacityValue == null)
        {
            colorValue = color.AsString();
        }
        else
        {
            var opacity = _opacity.GetValueOrDefault(opacityValue, "1");
            colorValue = color.AsStringWithOpacity(opacity);
        }

        foreach (var property in properties)
        {
            declarations.Add(new CssDeclaration(property, colorValue));
        }

        return declarations;
    }

    private ImmutableDictionary<string, CssColor> AllColors()
    {
        if (_completeColors == null)
        {
            var builder = ImmutableDictionary.CreateBuilder<string, CssColor>();
            builder.AddRange(_flattenedColors);
            builder.AddRange(AdditionalColors().Select(i => new KeyValuePair<string, CssColor>(i.Key, new CssColor(i.Value))));
            _completeColors = builder.ToImmutable();
        }

        return _completeColors;
    }

    private static ImmutableDictionary<string, string> AdditionalColors()
    {
        var b = ImmutableDictionary.CreateBuilder<string, string>();
        b.Add("inherit", "inherit");
        b.Add("current", "currentColor");
        b.Add("transparent", "transparent");
        return b.ToImmutable();
    }

    /// <summary>
    /// Checks if an arbitrary value is a valid color value and not a size value.
    /// </summary>
    /// <param name="value">The arbitrary value to check.</param>
    /// <returns>True if it's a valid color value, false otherwise.</returns>
    private static bool IsValidColorValue(string value)
    {
        // Check if it looks like a size value (ends with size units)
        var sizeUnits = new[] { "px", "em", "rem", "ex", "ch", "lh", "rlh", "vw", "vh", "vmin", "vmax", "vi", "vb", "dvw", "dvh", "lvw", "lvh", "svw", "svh", "%", "fr", "cm", "mm", "in", "pt", "pc", "deg", "rad", "turn", "grad", "s", "ms", "Hz", "kHz", "dpi", "dpcm", "dppx" };
        foreach (var unit in sizeUnits)
        {
            if (value.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
            {
                return false; // This looks like a size value, not a color
            }
        }

        // Check if it's a pure number (like "4", "10", etc.)
        if (double.TryParse(value, out _))
        {
            return false; // Pure numbers are size values, not colors
        }

        // If it starts with # or contains color function names, it's likely a color
        if (value.StartsWith('#') ||
            value.Contains("rgb", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("hsl", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("oklch", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("color", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Known color keywords
        var colorKeywords = new[] { "red", "green", "blue", "white", "black", "gray", "grey", "yellow", "orange", "purple", "pink", "brown", "cyan", "magenta", "lime", "navy", "teal", "silver", "maroon", "olive", "aqua", "fuchsia" };
        if (colorKeywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // If we can't determine it's clearly a color, assume it's not (to avoid conflicts with size values)
        return false;
    }

    /// <inheritdoc />
    public IEnumerable<CssRuleSet> GetAllRules()
    {
        foreach (var (ns, properties) in NamespaceToProperties)
        {
            foreach (var flattenedColor in AllColors())
            {
                var colorName = $"{ns}-{flattenedColor.Key}";
                yield return new CssRuleSet(colorName, GetDeclarations(flattenedColor.Value, null, properties));
            }
        }
    }
}