using System.Collections.Immutable;
using System.Globalization;
using MonorailCss.Ast;
using MonorailCss.Candidates;

namespace MonorailCss.Utilities.Base;

/// <summary>
/// Base class for functional utilities that resolve values from the theme or handle bare values.
/// Examples: opacity-50, z-10, order-1, grow, shrink.
/// </summary>
internal abstract class BaseFunctionalUtility : IUtility
{
    public virtual UtilityPriority Priority => UtilityPriority.NamespaceHandler;

    /// <summary>
    /// Gets the utility patterns this handles (e.g., ["opacity"] for opacity utilities).
    /// </summary>
    protected abstract string[] Patterns { get; }

    /// <summary>
    /// Gets the theme keys to check when resolving values.
    /// </summary>
    protected abstract string[] ThemeKeys { get; }

    /// <summary>
    /// Gets the default value to use when no value is provided (e.g., "1" for grow).
    /// Return null if no default value is supported.
    /// </summary>
    protected virtual string? DefaultValue => null;

    /// <summary>
    /// Gets a value indicating whether whether this utility supports negative values (e.g., -z-10).
    /// </summary>
    protected virtual bool SupportsNegative => false;

    public virtual string[] GetNamespaces() => ThemeKeys;

    public virtual string[] GetFunctionalRoots()
    {
        var roots = new List<string>();
        foreach (var pattern in Patterns)
        {
            roots.Add(pattern);
            if (SupportsNegative)
            {
                roots.Add($"-{pattern}");
            }
        }

        return roots.ToArray();
    }

    public virtual bool TryCompile(Candidate candidate, Theme.Theme theme, out ImmutableList<AstNode>? results)
    {
        results = null;

        if (candidate is not FunctionalUtility functionalUtility)
        {
            return false;
        }

        // Check if it's a negative utility
        var isNegative = SupportsNegative && functionalUtility.Root.StartsWith('-');
        var basePattern = isNegative ? functionalUtility.Root[1..] : functionalUtility.Root;

        if (!Patterns.Contains(basePattern))
        {
            return false;
        }

        // If no value provided, use default value if available
        if (functionalUtility.Value == null)
        {
            if (DefaultValue == null)
            {
                return false;
            }

            var declarations = GenerateDeclarations(basePattern, DefaultValue, candidate.Important);
            if (declarations.Count == 0)
            {
                return false;
            }

            results = declarations;
            return true;
        }

        if (!TryResolveValue(functionalUtility.Value, theme, isNegative, out var resolvedValue))
        {
            return false;
        }

        var finalDeclarations = GenerateDeclarations(basePattern, resolvedValue, candidate.Important);
        if (finalDeclarations.Count == 0)
        {
            return false;
        }

        results = finalDeclarations;
        return true;
    }

    /// <summary>
    /// Resolves a value using theme resolution or bare value handling.
    /// </summary>
    protected virtual bool TryResolveValue(CandidateValue value, Theme.Theme theme, bool isNegative, out string resolvedValue)
    {
        resolvedValue = string.Empty;

        // Handle arbitrary values
        if (value.Kind == ValueKind.Arbitrary)
        {
            var arbitrary = value.Value;

            // Apply negative if needed
            if (isNegative && IsValidArbitraryValue(arbitrary))
            {
                resolvedValue = $"calc(-1 * ({arbitrary}))";
                return true;
            }

            if (IsValidArbitraryValue(arbitrary))
            {
                resolvedValue = arbitrary;
                return true;
            }

            return false;
        }

        // Handle named values
        if (value.Kind == ValueKind.Named)
        {
            var key = value.Value;

            // Try bare value handling first
            var bareValue = HandleBareValue(key);
            if (bareValue != null)
            {
                // Apply negative if needed
                if (isNegative)
                {
                    // If it's already a number, negate it
                    if (double.TryParse(bareValue, out var numValue))
                    {
                        resolvedValue = (-numValue).ToString("G", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        resolvedValue = $"calc(-1 * ({bareValue}))";
                    }
                }
                else
                {
                    resolvedValue = bareValue;
                }

                return true;
            }

            // Try theme resolution
            foreach (var themeKey in ThemeKeys)
            {
                var fullKey = $"{themeKey}-{key}";
                if (theme.ContainsKey(fullKey))
                {
                    // ThemeVariableTrackingStage automatically tracks var() references
                    var themeValue = $"var({fullKey})";

                    // Apply negative if needed
                    if (isNegative)
                    {
                        resolvedValue = $"calc(-1 * {themeValue})";
                    }
                    else
                    {
                        resolvedValue = themeValue;
                    }

                    return true;
                }

                // Also try without dash for base namespace values
                var baseKey = $"{themeKey}{key}";
                if (theme.ContainsKey(baseKey))
                {
                    // ThemeVariableTrackingStage automatically tracks var() references
                    var themeValue = $"var({baseKey})";

                    // Apply negative if needed
                    if (isNegative)
                    {
                        resolvedValue = $"calc(-1 * {themeValue})";
                    }
                    else
                    {
                        resolvedValue = themeValue;
                    }

                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Handles bare values (e.g., "50" for opacity-50).
    /// Override this in derived classes to provide custom bare value handling.
    /// Return null if the value is not a valid bare value.
    /// </summary>
    protected virtual string? HandleBareValue(string value)
    {
        return null;
    }

    /// <summary>
    /// Validates arbitrary values. Override in derived classes for custom validation.
    /// </summary>
    protected virtual bool IsValidArbitraryValue(string value)
    {
        // Default implementation allows any non-empty value
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Generates CSS declarations based on the utility pattern and resolved value.
    /// Must be implemented by derived classes to handle specific CSS properties.
    /// </summary>
    protected abstract ImmutableList<AstNode> GenerateDeclarations(string pattern, string value, bool important);
}