using System.Collections.Immutable;

namespace MonorailCss.Theme;

/// <summary>
/// Manages design tokens and CSS custom properties for the framework.
/// In this modern implementation, all theme values are always output as CSS variables.
/// </summary>
public class Theme
{
    private readonly ImmutableDictionary<string, string> _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="Theme"/> class.
    /// </summary>
    public Theme()
        : this(ThemeDefaults.GetDefaults())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Theme"/> class.
    /// </summary>
    /// <param name="values">The theme values.</param>
    public Theme(ImmutableDictionary<string, string> values)
    {
        _values = values;
        Prefix = string.Empty;
        ProseCustomization = null;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Theme"/> class with default values merged with optional custom values.
    /// </summary>
    /// <param name="customValues">
    /// An optional dictionary of custom key-value pairs to override the default theme values. If null or empty, only default values are used.
    /// </param>
    /// <returns>
    /// A new instance of the <see cref="Theme"/> class with the merged theme values.
    /// </returns>
    public static Theme CreateWithDefaults(ImmutableDictionary<string, string>? customValues = null)
    {
        if (customValues == null || customValues.IsEmpty)
        {
            return new Theme();
        }

        // Merge defaults with custom values (custom values override)
        var mergedValues = ThemeDefaults.GetDefaults();
        foreach (var kvp in customValues)
        {
            mergedValues = mergedValues.SetItem(kvp.Key, kvp.Value);
        }

        return new Theme(mergedValues);
    }

    /// <summary>
    /// Creates and returns an empty instance of the <see cref="Theme"/> class with no predefined values.
    /// </summary>
    /// <returns>An empty <see cref="Theme"/> instance.</returns>
    public static Theme CreateEmpty()
    {
        return new Theme(ImmutableDictionary<string, string>.Empty);
    }

    /// <summary>
    /// Gets the immutable dictionary of theme values represented as design tokens.
    /// These values are utilized throughout the framework and are managed as CSS custom properties.
    /// </summary>
    public ImmutableDictionary<string, string> Values => _values;

    /// <summary>
    /// Gets the prefix used for namespacing CSS custom properties within the theme.
    /// This allows for easy differentiation and organization of CSS variables when used
    /// alongside other sets of styles or frameworks.
    /// </summary>
    public string Prefix { get; init; }

    /// <summary>
    /// Gets optional customization for prose utility styles.
    /// </summary>
    public ProseCustomization? ProseCustomization { get; init; }

    /// <summary>
    /// Adds a new key-value pair to the current theme and returns a new instance of the <see cref="Theme"/> class.
    /// </summary>
    /// <param name="key">The key representing the design token to be added.</param>
    /// <param name="value">The value associated with the design token.</param>
    /// <returns>A new instance of the <see cref="Theme"/> class with the specified key-value pair added.</returns>
    public Theme Add(string key, string value)
    {
        return new Theme(_values.SetItem(key, value))
        {
            Prefix = Prefix,
            ProseCustomization = ProseCustomization,
        };
    }

    /// <summary>
    /// Resolves a candidate value to a corresponding theme value or key.
    /// </summary>
    /// <param name="candidateValue">The candidate value to resolve.</param>
    /// <param name="themeKeys">An array of theme keys used for resolution.</param>
    /// <returns>
    /// A formatted string representing the resolved theme value using a CSS variable,
    /// or null if no resolution is found.
    /// </returns>
    public string? Resolve(string? candidateValue, string[] themeKeys)
    {
        var key = ResolveKey(candidateValue, themeKeys);
        if (key == null)
        {
            return null;
        }

        return $"var({EscapeKey(PrefixKey(key))})";
    }

    /// <summary>
    /// Resolves a candidate value into its corresponding theme value based on provided theme keys.
    /// </summary>
    /// <param name="candidateValue">The potential value to resolve, which might be a direct theme key or require further resolution.</param>
    /// <param name="themeKeys">An array of theme keys used to perform resolution if the candidate value is not a direct key.</param>
    /// <returns>
    /// The resolved theme value as a string, or null if the candidate value does not match any keys or cannot be resolved.
    /// </returns>
    public string? ResolveValue(string? candidateValue, string[] themeKeys)
    {
        // Handle direct key lookup
        if (candidateValue?.StartsWith("--") == true)
        {
            if (_values.TryGetValue(candidateValue, out var directValue))
            {
                return directValue;
            }

            return null;
        }

        var key = ResolveKey(candidateValue, themeKeys);
        if (key == null)
        {
            return null;
        }

        var value = _values.GetValueOrDefault(key);
        return value;
    }

    /// <summary>
    /// Resolves a key by matching a candidate value with available theme keys.
    /// </summary>
    /// <param name="candidateValue">The value for which a corresponding key needs to be resolved. Can include dot notation for legacy support.</param>
    /// <param name="themeKeys">An array of theme keys that are searched to determine a match.</param>
    /// <returns>
    /// The resolved key as a string if a match is found; otherwise, null.
    /// </returns>
    public string? ResolveKey(string? candidateValue, string[] themeKeys)
    {
        if (candidateValue == null)
        {
            // Try each theme key directly
            foreach (var themeKey in themeKeys)
            {
                if (_values.ContainsKey(themeKey))
                {
                    return themeKey;
                }
            }

            return null;
        }

        // Build candidate keys and check each
        foreach (var themeKey in themeKeys)
        {
            var key = $"{themeKey}-{candidateValue}";
            if (_values.ContainsKey(key))
            {
                return key;
            }

            // Legacy dot-to-underscore conversion for backwards compatibility
            if (candidateValue.Contains('.'))
            {
                var legacyKey = $"{themeKey}-{candidateValue.Replace('.', '_')}";
                if (_values.ContainsKey(legacyKey))
                {
                    return legacyKey;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether the theme contains a value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the theme.</param>
    /// <returns>True if the key exists in the theme; otherwise, false.</returns>
    public bool ContainsKey(string key)
    {
        return _values.ContainsKey(key);
    }

    /// <summary>
    /// Retrieves all keys within a specified namespace prefix in the theme.
    /// </summary>
    /// <param name="namespacePrefix">
    /// The namespace prefix to filter the keys. If the prefix does not start with '--', it is automatically prefixed with '--'.
    /// </param>
    /// <returns>
    /// An immutable list containing all keys within the given namespace that match the prefix pattern.
    /// </returns>
    public ImmutableList<string> KeysInNamespace(string namespacePrefix)
    {
        var prefix = namespacePrefix.StartsWith("--") ? namespacePrefix : $"--{namespacePrefix}";
        var prefixWithDash = $"{prefix}-";

        return _values.Keys
            .Where(key => key.StartsWith(prefixWithDash))
            .ToImmutableList();
    }

    /// <summary>
    /// Retrieves all key-value pairs in the theme that belong to the specified namespace.
    /// </summary>
    /// <param name="namespacePrefix">The namespace prefix used to filter the key-value pairs. If it does not start with "--", it will be prefixed accordingly.</param>
    /// <returns>An immutable dictionary containing all key-value pairs within the specified namespace.</returns>
    public ImmutableDictionary<string, string> Namespace(string namespacePrefix)
    {
        var prefix = namespacePrefix.StartsWith("--") ? namespacePrefix : $"--{namespacePrefix}";
        var prefixWithDash = $"{prefix}-";

        return _values
            .Where(kvp => kvp.Key.StartsWith(prefixWithDash))
            .ToImmutableDictionary();
    }

    /// <summary>
    /// Removes all theme entries within the specified namespace prefix and returns a new <see cref="Theme"/> instance without those entries.
    /// </summary>
    /// <param name="namespacePrefix">The prefix of the namespace to clear. Entries that start with this prefix will be removed.</param>
    /// <returns>A new <see cref="Theme"/> instance without the entries in the specified namespace.</returns>
    public Theme ClearNamespace(string namespacePrefix)
    {
        var prefix = namespacePrefix.StartsWith("--") ? namespacePrefix : $"--{namespacePrefix}";
        var prefixWithDash = $"{prefix}-";

        var newValues = _values
            .Where(kvp => !kvp.Key.StartsWith(prefixWithDash))
            .ToImmutableDictionary();

        return new Theme(newValues)
        {
            Prefix = Prefix,
            ProseCustomization = ProseCustomization,
        };
    }

    /// <summary>
    /// Adds a custom color palette to the theme.
    /// Standard shades should use keys: 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950.
    /// </summary>
    /// <param name="name">The name of the color palette (e.g., "brand", "primary").</param>
    /// <param name="shades">The color shades as an immutable dictionary.</param>
    /// <returns>A new theme instance with the color palette added.</returns>
    public Theme AddColorPalette(string name, ImmutableDictionary<string, string> shades)
    {
        var builder = _values.ToBuilder();

        foreach (var shade in shades)
        {
            var key = $"--color-{name}-{shade.Key}";
            builder[key] = shade.Value;
        }

        return new Theme(builder.ToImmutable())
        {
            Prefix = Prefix,
            ProseCustomization = ProseCustomization,
        };
    }

    /// <summary>
    /// Maps a color palette from a source alias to a target name by creating CSS variable mappings.
    /// </summary>
    /// <param name="alias">The source alias of an existing color palette to be mapped.</param>
    /// <param name="name">The target name to which the color palette will be mapped.</param>
    /// <returns>A new instance of the <see cref="Theme"/> class with the updated color mappings.</returns>
    public Theme MapColorPalette(string alias, string name)
    {
        var builder = _values.ToBuilder();

        string[] shades = ["50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950"];
        foreach (var shade in shades)
        {
            var key = $"--color-{name}-{shade}";
            builder[key] = $"var(--color-{alias}-{shade})";
        }

        return new Theme(builder.ToImmutable())
        {
            Prefix = Prefix,
            ProseCustomization = ProseCustomization,
        };
    }

    /// <summary>
    /// Adds a custom font family to the theme.
    /// </summary>
    /// <param name="name">The name of the font family (e.g., "display", "body").</param>
    /// <param name="fontStack">The font stack CSS value (e.g., "'Inter', sans-serif").</param>
    /// <returns>A new theme instance with the font family added.</returns>
    public Theme AddFontFamily(string name, string fontStack)
    {
        var key = $"--font-{name}";
        return new Theme(_values.SetItem(key, fontStack))
        {
            Prefix = Prefix,
            ProseCustomization = ProseCustomization,
        };
    }

    private string PrefixKey(string key)
    {
        if (string.IsNullOrEmpty(Prefix) || !key.StartsWith("--"))
        {
            return key;
        }

        return $"--{Prefix}-{key[2..]}";
    }

    private string EscapeKey(string key)
    {
        // CSS variable names don't need escaping in most cases
        // but we'll keep this for future extension if needed
        return key;
    }
}