using System.Collections.Immutable;
using System.Text;
using MonorailCss.Theme;

namespace MonorailCss.Css;

/// <summary>
/// Converts Theme objects and applies dictionaries to CSS format compatible with CssThemeParser.
/// </summary>
internal class ThemeToCssConverter
{
    private static readonly Lazy<ImmutableDictionary<string, string>> _defaultThemeValues =
        new Lazy<ImmutableDictionary<string, string>>(() =>
        {
            var defaults = ThemeDefaults.GetDefaults();
            var proseDefaults = ProseThemeDefaults.GetProseDefaults();
            return defaults.AddRange(proseDefaults);
        });

    /// <summary>
    /// Gets the combined default theme values.
    /// </summary>
    private static ImmutableDictionary<string, string> DefaultThemeValues => _defaultThemeValues.Value;

    /// <summary>
    /// Converts a Theme object to a CSS @theme block.
    /// </summary>
    /// <param name="theme">The theme to convert.</param>
    /// <param name="includeImport">Whether to include @import "tailwindcss" at the top.</param>
    /// <param name="excludeDefaults">Whether to exclude values that match defaults.</param>
    /// <returns>CSS string with @theme block.</returns>
    public string ConvertTheme(Theme.Theme theme, bool includeImport = false, bool excludeDefaults = true)
    {
        if (theme == null)
        {
            throw new ArgumentNullException(nameof(theme));
        }

        var sb = new StringBuilder();

        if (includeImport)
        {
            sb.AppendLine("@import \"tailwindcss\";");
            sb.AppendLine();
        }

        var valuesToWrite = excludeDefaults
            ? GetNonDefaultValues(theme.Values)
            : theme.Values;

        if (valuesToWrite.Count > 0)
        {
            sb.AppendLine("@theme {");

            foreach (var (key, value) in valuesToWrite.OrderBy(kvp => kvp.Key))
            {
                sb.AppendLine($"  {key}: {value};");
            }

            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts an applies dictionary to CSS rules with @apply directives.
    /// </summary>
    /// <param name="applies">Dictionary of selector to utility classes.</param>
    /// <returns>CSS string with component rules.</returns>
    public string ConvertApplies(ImmutableDictionary<string, string> applies)
    {
        if (applies.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();

        foreach (var (selector, utilities) in applies.OrderBy(kvp => kvp.Key))
        {
            sb.AppendLine($"{selector} {{");
            sb.AppendLine($"  @apply {utilities};");
            sb.AppendLine("}");

            // Add blank line between rules for readability
            if (selector != applies.Keys.Last())
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts both a Theme and applies dictionary to a complete CSS file.
    /// </summary>
    /// <param name="theme">The theme to convert.</param>
    /// <param name="applies">The applies dictionary to convert.</param>
    /// <param name="includeImport">Whether to include @import "tailwindcss" at the top.</param>
    /// <param name="excludeDefaults">Whether to exclude values that match defaults.</param>
    /// <returns>Complete CSS string.</returns>
    public string Convert(Theme.Theme theme, ImmutableDictionary<string, string>? applies = null, bool includeImport = true, bool excludeDefaults = true)
    {
        if (theme == null)
        {
            throw new ArgumentNullException(nameof(theme));
        }

        var sb = new StringBuilder();

        // Add import if requested
        if (includeImport)
        {
            sb.AppendLine("@import \"tailwindcss\";");
            sb.AppendLine();
        }

        // Add theme block if there are theme values
        var valuesToWrite = excludeDefaults
            ? GetNonDefaultValues(theme.Values)
            : theme.Values;

        if (valuesToWrite.Count > 0)
        {
            sb.AppendLine("@theme {");

            foreach (var (key, value) in valuesToWrite.OrderBy(kvp => kvp.Key))
            {
                sb.AppendLine($"  {key}: {value};");
            }

            sb.AppendLine("}");

            // Add blank line after theme if there are applies
            if (applies != null && applies.Count > 0)
            {
                sb.AppendLine();
            }
        }

        // Add component rules if there are applies
        if (applies != null && applies.Count > 0)
        {
            foreach (var (selector, utilities) in applies.OrderBy(kvp => kvp.Key))
            {
                sb.AppendLine($"{selector} {{");
                sb.AppendLine($"  @apply {utilities};");
                sb.AppendLine("}");

                // Add blank line between rules for readability
                if (selector != applies.Keys.Last())
                {
                    sb.AppendLine();
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts CssFrameworkSettings to a CSS string.
    /// </summary>
    /// <param name="settings">The framework settings to convert.</param>
    /// <param name="includeImport">Whether to include @import "tailwindcss" at the top.</param>
    /// <param name="excludeDefaults">Whether to exclude values that match defaults.</param>
    /// <returns>CSS string representation of the settings.</returns>
    public string ConvertSettings(CssFrameworkSettings settings, bool includeImport = true, bool excludeDefaults = true)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        return Convert(settings.Theme, settings.Applies, includeImport, excludeDefaults);
    }

    /// <summary>
    /// Filters out theme values that match the defaults.
    /// </summary>
    /// <param name="themeValues">The theme values to filter.</param>
    /// <returns>Dictionary containing only non-default values.</returns>
    private ImmutableDictionary<string, string> GetNonDefaultValues(ImmutableDictionary<string, string> themeValues)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>();

        foreach (var (key, value) in themeValues)
        {
            // Only include if the key doesn't exist in defaults or the value is different
            if (!DefaultThemeValues.TryGetValue(key, out var defaultValue) || defaultValue != value)
            {
                builder.Add(key, value);
            }
        }

        return builder.ToImmutable();
    }
}