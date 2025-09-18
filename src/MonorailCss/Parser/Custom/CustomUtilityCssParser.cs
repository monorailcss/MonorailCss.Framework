using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace MonorailCss.Parser.Custom;

/// <summary>
/// Parser for CSS @utility directives that define custom utilities.
/// </summary>
internal partial class CustomUtilityCssParser
{
    private static readonly Regex _utilityBlockRegex = UtilityBlockRegexDefinition();

    private static readonly Regex _nestedSelectorRegex = NestedSelectorRegexDefinition();

    private static readonly Regex _declarationRegex = DeclarationRegexDefinition();

    private static readonly Regex _varReferenceRegex = VarReferenceRegexDefinition();

    /// <summary>
    /// Parses CSS containing @utility directives and returns a collection of utility definitions.
    /// </summary>
    /// <param name="css">The CSS string to parse.</param>
    /// <returns>A collection of parsed utility definitions.</returns>
    public IEnumerable<UtilityDefinition> ParseCustomUtilities(string css)
    {
        if (string.IsNullOrWhiteSpace(css))
        {
            yield break;
        }

        var matches = _utilityBlockRegex.Matches(css);

        foreach (Match match in matches)
        {
            if (match.Success && match.Groups.Count >= 3)
            {
                var pattern = match.Groups[1].Value.Trim();
                var content = match.Groups[2].Value;

                var definition = ParseUtilityBlock(pattern, content);
                if (definition != null)
                {
                    yield return definition;
                }
            }
        }
    }

    /// <summary>
    /// Parses a single @utility block into a UtilityDefinition.
    /// </summary>
    private UtilityDefinition? ParseUtilityBlock(string pattern, string content)
    {
        if (string.IsNullOrWhiteSpace(pattern) || string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var definition = new UtilityDefinition
        {
            Pattern = pattern,
            IsWildcard = pattern.Contains('*'),
        };

        // Remove nested selectors from content to parse root-level declarations first
        var rootContent = _nestedSelectorRegex.Replace(content, string.Empty);
        var rootDeclarations = ParseDeclarations(rootContent);
        definition.Declarations = rootDeclarations.ToImmutableList();

        // Extract nested selectors
        var nestedSelectors = new List<NestedSelector>();
        var nestedMatches = _nestedSelectorRegex.Matches(content);

        foreach (Match nestedMatch in nestedMatches)
        {
            if (nestedMatch.Success && nestedMatch.Groups.Count >= 3)
            {
                var selector = nestedMatch.Groups[1].Value.Trim();
                var nestedContent = nestedMatch.Groups[2].Value;
                var nestedDeclarations = ParseDeclarations(nestedContent);

                if (nestedDeclarations.Any())
                {
                    nestedSelectors.Add(new NestedSelector(selector, nestedDeclarations.ToImmutableList()));
                }
            }
        }

        definition.NestedSelectors = nestedSelectors.ToImmutableList();

        // Extract CSS custom property dependencies
        var customProps = ExtractCustomPropertyDependencies(content);
        definition.CustomPropertyDependencies = customProps.ToImmutableList();

        return definition;
    }

    /// <summary>
    /// Parses CSS declarations from a content string.
    /// </summary>
    private List<CssDeclaration> ParseDeclarations(string content)
    {
        var declarations = new List<CssDeclaration>();

        if (string.IsNullOrWhiteSpace(content))
        {
            return declarations;
        }

        var matches = _declarationRegex.Matches(content);

        foreach (Match match in matches)
        {
            if (match.Success && match.Groups.Count >= 3)
            {
                var property = match.Groups[1].Value.Trim();
                var value = match.Groups[2].Value.Trim();

                if (!string.IsNullOrWhiteSpace(property) && !string.IsNullOrWhiteSpace(value))
                {
                    declarations.Add(new CssDeclaration(property, value));
                }
            }
        }

        return declarations;
    }

    /// <summary>
    /// Extracts CSS custom property dependencies from the content.
    /// </summary>
    private List<string> ExtractCustomPropertyDependencies(string content)
    {
        var customProps = new HashSet<string>();

        // Find properties being set as custom properties (--property-name)
        var setPropertyMatches = SetPropertyMatchesRegexDefinition().Matches(content);
        foreach (Match match in setPropertyMatches)
        {
            if (match.Success)
            {
                customProps.Add(match.Groups[1].Value);
            }
        }

        // Find properties referenced via var()
        var varMatches = _varReferenceRegex.Matches(content);
        foreach (Match match in varMatches)
        {
            if (match.Success && match.Groups.Count >= 2)
            {
                var varContent = match.Groups[1].Value.Trim();

                // Extract the property name from var() - it might have fallbacks
                var propName = varContent.Split(',')[0].Trim();
                if (propName.StartsWith("--"))
                {
                    customProps.Add(propName);
                }
            }
        }

        return customProps.ToList();
    }

    /// <summary>
    /// Validates that the specified utility definition conforms to the required format and structure.
    /// </summary>
    /// <param name="definition">The utility definition to validate.</param>
    /// <returns>True if the utility definition is well-formed, otherwise false.</returns>
    public bool ValidateUtilityDefinition(UtilityDefinition definition)
    {
        // Pattern must be non-empty and follow naming conventions
        if (string.IsNullOrWhiteSpace(definition.Pattern))
        {
            return false;
        }

        // Must have at least one declaration or nested selector
        if (!definition.Declarations.Any() && !definition.NestedSelectors.Any())
        {
            return false;
        }

        // Validate pattern format (lowercase, hyphens, optional wildcards)
        if (!ValidPatternRegexDefinition().IsMatch(definition.Pattern))
        {
            return false;
        }

        // Validate wildcard usage (only at the end of segments)
        if (definition.IsWildcard)
        {
            var parts = definition.Pattern.Split('-');
            foreach (var part in parts)
            {
                if (part.Contains('*') && part != "*")
                {
                    // Wildcard must be the entire segment
                    return false;
                }
            }
        }

        return true;
    }

    [GeneratedRegex(@"@utility\s+([a-z0-9\-\*]+)\s*\{((?:[^{}]|\{[^}]*\})*)\}", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline, "en-US")]
    private static partial Regex UtilityBlockRegexDefinition();
    [GeneratedRegex(@"\s*(&[^{]+)\s*\{([^}]*)\}", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex NestedSelectorRegexDefinition();
    [GeneratedRegex(@"([a-z\-]+(?:\-[a-z\-]+)*)\s*:\s*([^;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex DeclarationRegexDefinition();
    [GeneratedRegex(@"var\(([^)]+)\)", RegexOptions.Compiled)]
    private static partial Regex VarReferenceRegexDefinition();
    [GeneratedRegex(@"(--[a-z\-]+(?:\-[a-z\-]+)*)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex SetPropertyMatchesRegexDefinition();
    [GeneratedRegex(@"^[a-z0-9\-\*]+$")]
    private static partial Regex ValidPatternRegexDefinition();
}