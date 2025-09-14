using System.Diagnostics.CodeAnalysis;

namespace MonorailCss.Parser;

/// <summary>
/// Matches and classifies utility types (static, functional, arbitrary property).
/// </summary>
internal sealed class UtilityMatcher
{
    private readonly UtilityRegistry _utilityRegistry;
    private readonly ArbitraryValueParser _arbitraryValueParser;

    public UtilityMatcher(UtilityRegistry utilityRegistry, ArbitraryValueParser arbitraryValueParser)
    {
        _utilityRegistry = utilityRegistry;
        _arbitraryValueParser = arbitraryValueParser;
    }

    /// <summary>
    /// Result of matching a utility string.
    /// </summary>
    internal record UtilityMatch
    {
        public UtilityType Type { get; init; }
        public string Root { get; init; } = string.Empty;
        public string? Value { get; init; }
        public string? Property { get; init; }
    }

    public enum UtilityType
    {
        Unknown,
        Static,
        Functional,
        ArbitraryProperty,
        ParenthesesShorthand,
    }

    /// <summary>
    /// Matches a utility string and determines its type and components.
    /// </summary>
    public UtilityMatch Match(string utility)
    {
        // Check if it's a known static utility (exact match)
        if (IsStaticUtility(utility))
        {
            return new UtilityMatch
            {
                Type = UtilityType.Static,
                Root = utility,
            };
        }

        // Check for arbitrary property [property:value]
        if (TryMatchArbitraryProperty(utility, out var propertyName, out var propertyValue))
        {
            return new UtilityMatch
            {
                Type = UtilityType.ArbitraryProperty,
                Property = propertyName,
                Value = propertyValue,
            };
        }

        // Check for parentheses shorthand first (e.g., bg-(--my-color))
        if (utility.EndsWith(')'))
        {
            var parenIndex = utility.IndexOf('(');
            if (parenIndex > 0 && utility[parenIndex - 1] == '-')
            {
                var root = utility[..(parenIndex - 1)];
                var parenContent = utility[(parenIndex + 1)..^1];

                // Parse the parentheses content
                var parsed = _arbitraryValueParser.Parse(parenContent, ArbitraryValueType.Parentheses);
                if (parsed.IsValid)
                {
                    return new UtilityMatch
                    {
                        Type = UtilityType.ParenthesesShorthand,
                        Root = root,
                        Value = parsed.Value,
                    };
                }
            }
        }

        // Try to find functional utilities using the FindRoots algorithm
        var roots = FindRoots(utility);
        if (roots.Count > 0)
        {
            var (root, value) = roots[0]; // Take the first match
            return new UtilityMatch
            {
                Type = UtilityType.Functional,
                Root = root,
                Value = value,
            };
        }

        return new UtilityMatch
        {
            Type = UtilityType.Unknown,
            Root = utility,
        };
    }

    private bool IsStaticUtility(string name)
    {
        return _utilityRegistry.StaticUtilitiesLookup.ContainsKey(name);
    }

    private bool HasFunctionalRoot(string root)
    {
        return _utilityRegistry.FunctionalRoots.Contains(root);
    }

    /// <summary>
    /// Tries to parse an arbitrary property from a class name.
    /// </summary>
    private bool TryMatchArbitraryProperty(string className, [NotNullWhen(true)] out string? propertyName, [NotNullWhen(true)] out string? value)
    {
        propertyName = null;
        value = null;

        if (!className.StartsWith('[') || !className.EndsWith(']'))
        {
            return false;
        }

        var content = className[1..^1];

        // Validate for empty or invalid content
        if (string.IsNullOrWhiteSpace(content) || content == "_")
        {
            return false; // Empty or underscore-only arbitrary value
        }

        var colonIndex = content.IndexOf(':');

        if (colonIndex <= 0)
        {
            return false;
        }

        propertyName = content[..colonIndex];
        value = content[(colonIndex + 1)..];

        // Reject arbitrary properties starting with uppercase
        if (propertyName.Length > 0 && char.IsUpper(propertyName[0]))
        {
            return false;
        }

        // Parse the value for any special handling
        var parsed = _arbitraryValueParser.Parse(value, ArbitraryValueType.Brackets);
        if (!parsed.IsValid)
        {
            return false;
        }

        value = parsed.Value!;
        return true;
    }

    /// <summary>
    /// Finds potential root-value pairs from a utility string.
    /// </summary>
    private List<(string Root, string? Value)> FindRoots(string input)
    {
        var results = new List<(string Root, string? Value)>();

        // If there is an exact match as a functional utility, that's the root
        if (HasFunctionalRoot(input))
        {
            results.Add((input, null));
            return results;
        }

        // Handle negative values by checking all functional roots if input starts with dash
        if (input.StartsWith("-"))
        {
            var inputWithoutDash = input[1..];

            // Check if the input without the dash matches any registered functional roots
            foreach (var functionalRoot in _utilityRegistry.FunctionalRoots
                .Where(r => !r.StartsWith("-"))
                .OrderByDescending(r => r.Length))
            {
                // Only check positive roots
                // Check longer roots first to handle "hue-rotate" before "hue"
                if (inputWithoutDash.StartsWith(functionalRoot + "-"))
                {
                    var value = inputWithoutDash[(functionalRoot.Length + 1)..];
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Return the negative root (e.g., "-hue-rotate" instead of "hue-rotate")
                        results.Add(($"-{functionalRoot}", value));
                        return results;
                    }
                }
            }
        }

        // Special multi-dash patterns that should be treated as single roots
        // These utilities have dashes in their base name (e.g., space-x, divide-y)
        // Also include utilities that might support fractions even if not yet implemented
        var multiDashPatterns = new[]
        {
            "space-x", "space-y", "divide-x", "divide-y",
            "translate-x", "translate-y", "-translate-x", "-translate-y",
            "inset-x", "inset-y",
        };
        foreach (var pattern in multiDashPatterns)
        {
            if (!input.StartsWith(pattern + "-"))
            {
                continue;
            }

            if (!HasFunctionalRoot(pattern))
            {
                continue;
            }

            var value = input[(pattern.Length + 1)..];
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            results.Add((pattern, value));
            return results;
        }

        // Otherwise test every permutation of the input by iteratively removing
        // everything after each dash, starting from the last one
        var idx = input.LastIndexOf('-');

        // Determine the root and value by testing permutations
        // For `bg-red-500`:
        // - Try `bg-red` as root with `500` as value
        // - Try `bg` as root with `red-500` as value
        while (idx > 0)
        {
            var maybeRoot = input[..idx];

            if (HasFunctionalRoot(maybeRoot))
            {
                var value = input[(idx + 1)..];

                // If the leftover value is empty (e.g., `bg-`), skip it
                if (string.IsNullOrEmpty(value))
                {
                    break;
                }

                results.Add((maybeRoot, value));

                // Only return the first match for now
                return results;
            }

            idx = input.LastIndexOf('-', idx - 1);
        }

        // Special case: if nothing matched and input contains a dash, bracket, or parenthesis,
        // treat the first segment as root (for unknown utilities)
        if (input.Contains('-'))
        {
            var firstDashIndex = input.IndexOf('-');
            if (firstDashIndex <= 0)
            {
                return results;
            }

            var root = input[..firstDashIndex];
            var value = input[(firstDashIndex + 1)..];
            if (!string.IsNullOrEmpty(value))
            {
                results.Add((root, value));
            }
        }
        else if (input.Contains('['))
        {
            var bracketIndex = input.IndexOf('[');
            if (bracketIndex <= 0)
            {
                return results;
            }

            var root = input[..bracketIndex];
            var value = input[bracketIndex..];
            results.Add((root, value));
        }
        else if (input.Contains('('))
        {
            var parenIndex = input.IndexOf('(');
            if (parenIndex <= 0)
            {
                return results;
            }

            var root = input[..parenIndex];
            var value = input[parenIndex..];
            results.Add((root, value));
        }

        return results;
    }
}