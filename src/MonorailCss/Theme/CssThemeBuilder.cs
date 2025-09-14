using System.Collections.Immutable;
using MonorailCss.Css;

namespace MonorailCss.Theme;

/// <summary>
/// Builds a Theme by merging CSS-defined theme variables with existing C# themes.
/// Supports chaining multiple theme sources in order.
/// </summary>
internal class CssThemeBuilder
{
    private readonly CssThemeParser _parser = new();

    /// <summary>
    /// Merges CSS theme sources with an existing theme.
    /// </summary>
    /// <param name="baseTheme">The base theme to merge with.</param>
    /// <param name="cssSources">CSS source strings containing theme definitions.</param>
    /// <returns>A new theme with merged values.</returns>
    public Theme MergeWithCssSources(Theme baseTheme, IList<string> cssSources)
    {
        if (baseTheme == null)
        {
            throw new ArgumentNullException(nameof(baseTheme));
        }

        if (!cssSources.Any())
        {
            return baseTheme;
        }

        var mergedTheme = baseTheme;

        foreach (var cssSource in cssSources)
        {
            if (string.IsNullOrWhiteSpace(cssSource))
            {
                continue;
            }

            var parseResult = _parser.Parse(cssSource);

            // Apply theme variables
            foreach (var (key, value) in parseResult.ThemeVariables)
            {
                mergedTheme = mergedTheme.Add(key, value);
            }
        }

        return mergedTheme;
    }

    /// <summary>
    /// Extracts component rules (applies) from CSS sources.
    /// </summary>
    /// <param name="cssSources">CSS source strings containing component definitions.</param>
    /// <returns>Dictionary of selector to utility classes.</returns>
    public ImmutableDictionary<string, string> ExtractApplies(IList<string> cssSources)
    {
        if (!cssSources.Any())
        {
            return ImmutableDictionary<string, string>.Empty;
        }

        var applies = ImmutableDictionary.CreateBuilder<string, string>();

        foreach (var cssSource in cssSources)
        {
            if (string.IsNullOrWhiteSpace(cssSource))
            {
                continue;
            }

            var parseResult = _parser.Parse(cssSource);

            // Merge component rules (last one wins for duplicate selectors)
            foreach (var (selector, utilities) in parseResult.ComponentRules)
            {
                applies[selector] = utilities;
            }
        }

        return applies.ToImmutable();
    }

    /// <summary>
    /// Processes CSS sources and returns both theme and applies.
    /// </summary>
    /// <param name="baseTheme">The base theme to merge with.</param>
    /// <param name="baseApplies">The base applies to merge with.</param>
    /// <param name="cssSources">CSS source strings to process.</param>
    /// <returns>Tuple of merged theme and applies.</returns>
    public (Theme Theme, ImmutableDictionary<string, string> Applies) ProcessCssSources(
        Theme baseTheme,
        ImmutableDictionary<string, string> baseApplies,
        IList<string> cssSources)
    {
        if (baseTheme == null)
        {
            throw new ArgumentNullException(nameof(baseTheme));
        }

        if (!cssSources.Any())
        {
            return (baseTheme, baseApplies);
        }

        var mergedTheme = baseTheme;
        var mergedApplies = baseApplies.ToBuilder();

        foreach (var cssSource in cssSources)
        {
            if (string.IsNullOrWhiteSpace(cssSource))
            {
                continue;
            }

            var parseResult = _parser.Parse(cssSource);

            // Apply theme variables
            foreach (var (key, value) in parseResult.ThemeVariables)
            {
                mergedTheme = mergedTheme.Add(key, value);
            }

            // Merge component rules (last one wins for duplicate selectors)
            foreach (var (selector, utilities) in parseResult.ComponentRules)
            {
                mergedApplies[selector] = utilities;
            }
        }

        return (mergedTheme, mergedApplies.ToImmutable());
    }
}