using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using MonorailCss.Ast;
using MonorailCss.Candidates;
using MonorailCss.Core;
using MonorailCss.Utilities;
using MonorailCss.Utilities.Resolvers;

namespace MonorailCss.Parser.Custom;

/// <summary>
/// A custom utility implementation for dynamic CSS patterns defined via @utility directives with wildcards.
/// Handles pattern matching and value resolution from theme or arbitrary values.
/// </summary>
public partial class DynamicCustomUtility : IUtility
{
    // Static regex cache for performance optimization
    // Pattern string -> compiled Regex
    private static readonly Dictionary<string, Regex> _regexCache = new();
    private static readonly object _regexCacheLock = new();

    private readonly UtilityDefinition _definition;
    private readonly Regex _patternRegex;
    private readonly string _basePattern;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicCustomUtility"/> class.
    /// </summary>
    /// <param name="definition">The utility definition parsed from CSS.</param>
    public DynamicCustomUtility(UtilityDefinition definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (!definition.IsWildcard)
        {
            throw new ArgumentException("Dynamic custom utilities must use wildcard patterns", nameof(definition));
        }

        _definition = definition;

        // Extract base pattern (everything before the first wildcard)
        var wildcardIndex = definition.Pattern.IndexOf('*');
        _basePattern = wildcardIndex > 0 ? definition.Pattern[..wildcardIndex].TrimEnd('-') : definition.Pattern;

        // Get or create cached regex for this pattern
        _patternRegex = GetOrCreateCachedRegex(definition.Pattern);
    }

    /// <summary>
    /// Gets or creates a cached regex pattern for improved performance.
    /// Thread-safe implementation to avoid concurrent dictionary modifications.
    /// </summary>
    private static Regex GetOrCreateCachedRegex(string pattern)
    {
        // Convert pattern to regex pattern string
        var regexPattern = "^" + Regex.Escape(pattern).Replace(@"\*", "(.+)") + "$";

        // Check cache first (optimistic path, no lock)
        if (_regexCache.TryGetValue(regexPattern, out var cachedRegex))
        {
            return cachedRegex;
        }

        // If not found, acquire lock and check again (double-check pattern)
        lock (_regexCacheLock)
        {
            // Another thread might have added it while we were waiting for the lock
            if (_regexCache.TryGetValue(regexPattern, out cachedRegex))
            {
                return cachedRegex;
            }

            // Create and cache the new regex
            var newRegex = new Regex(regexPattern, RegexOptions.Compiled);
            _regexCache[regexPattern] = newRegex;
            return newRegex;
        }
    }

    /// <summary>
    /// Gets the utility priority. Dynamic custom utilities use NamespaceHandler priority.
    /// </summary>
    public UtilityPriority Priority => UtilityPriority.NamespaceHandler;

    /// <summary>
    /// Gets the utility layer. Custom utilities belong to the Utility layer.
    /// </summary>
    public UtilityLayer Layer => UtilityLayer.Utility;

    /// <summary>
    /// Retrieves the namespaces associated with the utility.
    /// Returns the custom property dependencies from the definition.
    /// </summary>
    /// <returns>
    /// An array of strings representing the namespaces.
    /// </returns>
    public string[] GetNamespaces() => _definition.CustomPropertyDependencies.ToArray();

    /// <summary>
    /// Gets the functional roots that this utility handles.
    /// Returns the base pattern for the parser to recognize.
    /// </summary>
    /// <returns>The functional roots as an array.</returns>
    public string[] GetFunctionalRoots() => [_basePattern];

    /// <summary>
    /// Attempts to compile the specified candidate into Abstract Syntax Tree (AST) nodes
    /// based on the provided theme and utility definition.
    /// </summary>
    /// <param name="candidate">The candidate to be compiled.</param>
    /// <param name="theme">The theme to assist in compilation and determining utility rules.</param>
    /// <param name="results">When this method returns, contains the list of compiled AST nodes if the compilation is successful, or null if it fails.</param>
    /// <returns>True if the candidate was successfully compiled into AST nodes; otherwise, false.</returns>
    public bool TryCompile(Candidate candidate, Theme.Theme theme, [NotNullWhen(true)] out ImmutableList<AstNode>? results)
    {
        results = null;

        // Extract the utility name from the candidate
        string utilityName = GetUtilityName(candidate);

        // Try to match the pattern
        var match = _patternRegex.Match(utilityName);
        if (!match.Success)
        {
            return false;
        }

        // Extract captured values from the pattern match
        var capturedValues = new List<string>();
        for (int i = 1; i < match.Groups.Count; i++)
        {
            capturedValues.Add(match.Groups[i].Value);
        }

        if (capturedValues.Count == 0)
        {
            return false;
        }

        var nodes = new List<AstNode>();

        // Process root-level declarations with value substitution
        foreach (var declaration in _definition.Declarations)
        {
            var processedValue = ProcessValueWithSubstitutions(declaration.Value, capturedValues, theme, candidate);
            if (processedValue != null)
            {
                nodes.Add(new Declaration(declaration.Property, processedValue, candidate.Important));
            }
        }

        // Process nested selectors with value substitution
        foreach (var nestedSelector in _definition.NestedSelectors)
        {
            var nestedDeclarations = new List<AstNode>();
            foreach (var declaration in nestedSelector.Declarations)
            {
                var processedValue = ProcessValueWithSubstitutions(declaration.Value, capturedValues, theme, candidate);
                if (processedValue != null)
                {
                    nestedDeclarations.Add(new Declaration(declaration.Property, processedValue, candidate.Important));
                }
            }

            if (nestedDeclarations.Count > 0)
            {
                // Process selector (remove & prefix if present)
                var selector = nestedSelector.Selector;
                if (selector.StartsWith("&"))
                {
                    selector = selector.Substring(1);
                }

                nodes.Add(new NestedRule(selector, nestedDeclarations.ToImmutableList()));
            }
        }

        if (nodes.Count == 0)
        {
            return false;
        }

        results = nodes.ToImmutableList();
        return true;
    }

    /// <summary>
    /// Extracts the utility name from a candidate.
    /// </summary>
    private string GetUtilityName(Candidate candidate)
    {
        return candidate switch
        {
            StaticUtility staticUtil => staticUtil.Root,
            FunctionalUtility funcUtil when funcUtil.Value != null =>
                funcUtil.Value.Kind == ValueKind.Arbitrary
                    ? $"{funcUtil.Root}-[{funcUtil.Value.Value}]"
                    : $"{funcUtil.Root}-{funcUtil.Value.Value}",
            FunctionalUtility funcUtil => funcUtil.Root,
            _ => candidate.Raw,
        };
    }

    /// <summary>
    /// Processes a value string with substitutions for captured wildcard values.
    /// Handles --value() function calls and direct replacements.
    /// </summary>
    private string? ProcessValueWithSubstitutions(string value, List<string> capturedValues, Theme.Theme theme, Candidate candidate)
    {
        // Handle --value() function
        if (value.Contains("--value("))
        {
            return ProcessValueFunction(value, capturedValues[0], theme, candidate);
        }

        // Direct substitution with first captured value
        if (value.Contains("*") && capturedValues.Count > 0)
        {
            // Replace * with the captured value
            return value.Replace("*", ResolveValue(capturedValues[0], theme, candidate));
        }

        // Return the value as-is if no substitution needed
        return value;
    }

    /// <summary>
    /// Processes a --value() function call with theme resolution.
    /// </summary>
    private string? ProcessValueFunction(string value, string capturedValue, Theme.Theme theme, Candidate candidate)
    {
        // Extract the argument from --value() function
        var valueMatch = ExtractArgumentFromValueFunctionRegexDefinition().Match(value);
        if (!valueMatch.Success)
        {
            return value;
        }

        var themeKeyPattern = valueMatch.Groups[1].Value;

        // Replace * with captured value in the theme key pattern
        var themeKey = themeKeyPattern.Replace("*", capturedValue);

        // Handle arbitrary values in square brackets
        if (candidate is FunctionalUtility funcUtil && funcUtil.Value?.Kind == ValueKind.Arbitrary)
        {
            // For arbitrary values, check if we should parse complex functions
            var arbitraryValue = funcUtil.Value.Value;

            // Use ValueParser for complex values if needed
            if (arbitraryValue.Contains("(") && arbitraryValue.Contains(")"))
            {
                var parsed = ValueParser.Parse(arbitraryValue);
                var processed = ValueParser.ToCss(parsed);
                return value.Replace(valueMatch.Value, processed);
            }

            return value.Replace(valueMatch.Value, arbitraryValue);
        }

        // Try to resolve from theme
        var resolvedValue = ResolveThemeValue(themeKey, theme);

        // Replace --value() with resolved value
        if (resolvedValue != null)
        {
            return value.Replace(valueMatch.Value, resolvedValue);
        }

        return null;
    }

    /// <summary>
    /// Resolves a value from theme or returns it directly if it's a valid CSS value.
    /// </summary>
    private string ResolveValue(string value, Theme.Theme theme, Candidate candidate)
    {
        // Handle arbitrary values
        if (candidate is FunctionalUtility funcUtil && funcUtil.Value?.Kind == ValueKind.Arbitrary)
        {
            return funcUtil.Value.Value;
        }

        // Create a candidate value for resolution
        var candidateValue = CandidateValue.Named(value);

        // Try to resolve as a color using the framework's color resolver
        // Use a simple color namespace chain
        string[] colorNamespaces = [NamespaceResolver.Color];
        if (ValueResolver.TryResolveColor(
            candidateValue,
            theme,
            colorNamespaces,
            out var colorResult))
        {
            return colorResult;
        }

        // Check if it's a valid CSS value using type inference
        var inferredType = ValueTypeInference.InferType(value);
        if (inferredType == ValueTypeInference.ValueType.Color ||
            inferredType == ValueTypeInference.ValueType.Length ||
            inferredType == ValueTypeInference.ValueType.Percentage ||
            inferredType == ValueTypeInference.ValueType.Number)
        {
            return value;
        }

        // Return as-is for other values
        return value;
    }

    /// <summary>
    /// Resolves a theme key to its value.
    /// </summary>
    private string? ResolveThemeValue(string themeKey, Theme.Theme theme)
    {
        // Remove -- prefix if present for theme resolution
        var cleanKey = themeKey.StartsWith("--") ? themeKey[2..] : themeKey;

        // Split the key to determine potential namespaces
        var parts = cleanKey.Split('-');
        if (parts.Length == 0)
        {
            return null;
        }

        // Try to resolve using the theme's built-in resolution
        // First try as a color
        string[] colorNamespaces = [NamespaceResolver.Color];
        var resolved = theme.Resolve(string.Join("-", parts.Skip(1)), colorNamespaces);
        if (resolved != null)
        {
            return resolved;
        }

        // Try other common namespaces
        resolved = theme.Resolve(cleanKey, []);
        if (resolved != null)
        {
            return resolved;
        }

        // Direct lookup with -- prefix
        var fullKey = themeKey.StartsWith("--") ? themeKey : $"--{themeKey}";
        if (theme.ContainsKey(fullKey))
        {
            return $"var({fullKey})";
        }

        return null;
    }

    [GeneratedRegex(@"--value\(([^)]+)\)")]
    private static partial Regex ExtractArgumentFromValueFunctionRegexDefinition();
}