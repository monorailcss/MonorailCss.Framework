using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace MonorailCss.Theme;

/// <summary>
/// Tracks theme variable usage during CSS processing.
/// This class is separate from Theme to keep Theme immutable and stateless.
/// </summary>
internal partial class ThemeUsageTracker
{
    private readonly HashSet<string> _usedValues = new();
    private readonly Theme _theme;

    public ThemeUsageTracker(Theme theme)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }

    /// <summary>
    /// Marks a value as used for optimization purposes.
    /// Also tracks any variables referenced in the value.
    /// </summary>
    public void MarkUsed(string key)
    {
        if (_usedValues.Add(key))
        {
            // If this is a new addition, also track any variables it references
            if (_theme.Values.TryGetValue(key, out var value))
            {
                TrackReferencedVariables(value);
            }
        }
    }

    /// <summary>
    /// Checks if a value has been used.
    /// </summary>
    public bool IsUsed(string key)
    {
        return _usedValues.Contains(key);
    }

    /// <summary>
    /// Gets all used values for optimization.
    /// </summary>
    public IEnumerable<string> GetUsedValues()
    {
        return _usedValues;
    }

    /// <summary>
    /// Gets all used values with their corresponding values in a single pass.
    /// This is more efficient than calling GetUsedValues() and then GetValue() for each key.
    /// </summary>
    public ImmutableDictionary<string, string> GetUsedValuesWithValues()
    {
        var result = ImmutableDictionary.CreateBuilder<string, string>();
        foreach (var key in _usedValues)
        {
            if (_theme.Values.TryGetValue(key, out var value))
            {
                result[key] = value;
            }
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Tracks any CSS variables referenced within a value string.
    /// This handles recursive tracking for mapped palettes.
    /// </summary>
    public void TrackReferencedVariables(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        // Parse for var(--variable-name) patterns
        var matches = VarPatternRegexDefinition().Matches(value);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var referencedVar = match.Groups[1].Value;
                MarkUsed(referencedVar);

                // Recursively track if this variable also references others
                if (_theme.Values.TryGetValue(referencedVar, out var referencedValue))
                {
                    TrackReferencedVariables(referencedValue);
                }
            }
        }
    }

    /// <summary>
    /// Resolves a theme value, always returning a CSS variable reference.
    /// Also tracks any referenced variables if the value itself contains var() references.
    /// </summary>
    public string? Resolve(string? candidateValue, string[] themeKeys)
    {
        var result = _theme.Resolve(candidateValue, themeKeys);
        if (result != null)
        {
            // Extract the key from the var() reference
            var match = VarPatternRegexDefinition().Match(result);
            if (match is { Success: true, Groups.Count: > 1 })
            {
                var key = match.Groups[1].Value;
                MarkUsed(key);

                // Also track any variables referenced by this key's value
                if (_theme.Values.TryGetValue(key, out var value))
                {
                    TrackReferencedVariables(value);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Resolves to the raw value (for cases where we need the actual value, not var()).
    /// Supports direct key lookup when candidateValue starts with "--".
    /// Also tracks any referenced variables in var() syntax.
    /// </summary>
    public string? ResolveValue(string? candidateValue, string[] themeKeys)
    {
        var result = _theme.ResolveValue(candidateValue, themeKeys);
        if (result != null)
        {
            // Track the key that was resolved
            if (candidateValue?.StartsWith("--") == true && _theme.Values.ContainsKey(candidateValue))
            {
                MarkUsed(candidateValue);
            }
            else
            {
                // Need to determine which key was actually resolved
                var key = _theme.ResolveKey(candidateValue, themeKeys);
                if (key != null)
                {
                    MarkUsed(key);
                }
            }

            // Track any referenced variables in the value
            TrackReferencedVariables(result);
        }

        return result;
    }

    [GeneratedRegex(@"var\((--[a-zA-Z0-9-]+)\)")]
    private static partial Regex VarPatternRegexDefinition();
}