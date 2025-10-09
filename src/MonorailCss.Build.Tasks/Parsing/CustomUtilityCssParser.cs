using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace MonorailCss.Build.Tasks.Parsing;

/// <summary>
/// Parser for custom utility definitions in CSS using @utility directives.
/// </summary>
internal partial class CustomUtilityCssParser
{
    private static readonly Regex _utilityBlockRegex = UtilityBlockRegexDefinition();
    private static readonly Regex _varFunctionRegex = VarFunctionRegexDefinition();
    private static readonly Regex _applyDirectiveRegex = ApplyDirectiveRegexDefinition();

    /// <summary>
    /// Parses CSS source and extracts custom utility definitions from @utility blocks.
    /// </summary>
    /// <param name="cssSource">The CSS source to parse.</param>
    /// <returns>List of parsed utility definitions.</returns>
    public IList<UtilityDefinition> ParseCustomUtilities(string cssSource)
    {
        if (string.IsNullOrWhiteSpace(cssSource))
        {
            return Array.Empty<UtilityDefinition>();
        }

        var utilities = new List<UtilityDefinition>();

        // Remove comments first
        cssSource = RemoveComments(cssSource);

        // Find all @utility blocks
        var matches = _utilityBlockRegex.Matches(cssSource);

        foreach (Match match in matches)
        {
            var utilityName = match.Groups[1].Value.Trim();
            var utilityContent = match.Groups[2].Value;

            // Parse the utility definition
            var definition = ParseUtilityBlock(utilityName, utilityContent);
            utilities.Add(definition);
        }

        return utilities;
    }

    private UtilityDefinition ParseUtilityBlock(string pattern, string content)
    {
        var declarations = new List<CssDeclaration>();
        var nestedSelectors = new List<NestedSelector>();
        var customProperties = new HashSet<string>();
        var applyUtilities = new List<string>();

        // Check if this is a wildcard pattern
        var isWildcard = pattern.Contains('*');

        // Extract @apply directives first
        var applyMatches = _applyDirectiveRegex.Matches(content);
        foreach (Match applyMatch in applyMatches)
        {
            var applyContent = applyMatch.Groups[1].Value.Trim();
            // Split utilities while preserving variant prefixes like "hover:"
            applyUtilities.AddRange(SplitUtilities(applyContent));
        }

        // Remove @apply directives from content before parsing declarations
        content = _applyDirectiveRegex.Replace(content, string.Empty);

        // Parse the content to extract declarations and nested selectors
        ParseBlockContent(content, declarations, nestedSelectors, customProperties);

        // Extract custom property dependencies from values
        foreach (var decl in declarations)
        {
            ExtractCustomProperties(decl.Value, customProperties);
        }

        foreach (var nested in nestedSelectors)
        {
            foreach (var decl in nested.Declarations)
            {
                ExtractCustomProperties(decl.Value, customProperties);
            }
        }

        return new UtilityDefinition
        {
            Pattern = pattern,
            IsWildcard = isWildcard,
            Declarations = declarations.ToImmutableList(),
            NestedSelectors = nestedSelectors.ToImmutableList(),
            CustomPropertyDependencies = customProperties.ToImmutableList(),
            ApplyUtilities = applyUtilities.ToImmutableList()
        };
    }

    private void ParseBlockContent(
        string content,
        List<CssDeclaration> declarations,
        List<NestedSelector> nestedSelectors,
        HashSet<string> customProperties)
    {
        var lines = content.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var currentNested = (string?)null;
        var nestedDeclarations = new List<CssDeclaration>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            // Check for nested selector start (e.g., "&::-webkit-scrollbar {")
            if (trimmed.StartsWith("&"))
            {
                var braceIndex = trimmed.IndexOf('{');
                if (braceIndex > 0)
                {
                    // Save previous nested selector if any
                    if (currentNested != null && nestedDeclarations.Count > 0)
                    {
                        nestedSelectors.Add(new NestedSelector(currentNested, nestedDeclarations.ToImmutableList()));
                        nestedDeclarations.Clear();
                    }

                    currentNested = trimmed[..braceIndex].Trim();
                    var afterBrace = trimmed[(braceIndex + 1)..].Trim();
                    if (!string.IsNullOrEmpty(afterBrace))
                    {
                        ParseDeclaration(afterBrace, nestedDeclarations, customProperties);
                    }
                    continue;
                }
            }

            // Check for nested selector end
            if (trimmed.Contains('}'))
            {
                var beforeBrace = trimmed[..trimmed.IndexOf('}')].Trim();
                if (!string.IsNullOrEmpty(beforeBrace))
                {
                    ParseDeclaration(beforeBrace, nestedDeclarations, customProperties);
                }

                if (currentNested != null && nestedDeclarations.Count > 0)
                {
                    nestedSelectors.Add(new NestedSelector(currentNested, nestedDeclarations.ToImmutableList()));
                    nestedDeclarations.Clear();
                    currentNested = null;
                }
                continue;
            }

            // Regular declaration
            if (currentNested != null)
            {
                ParseDeclaration(trimmed, nestedDeclarations, customProperties);
            }
            else
            {
                ParseDeclaration(trimmed, declarations, customProperties);
            }
        }

        // Handle any remaining nested selector
        if (currentNested != null && nestedDeclarations.Count > 0)
        {
            nestedSelectors.Add(new NestedSelector(currentNested, nestedDeclarations.ToImmutableList()));
        }
    }

    private void ParseDeclaration(string text, List<CssDeclaration> declarations, HashSet<string> customProperties)
    {
        var colonIndex = text.IndexOf(':');
        if (colonIndex <= 0)
        {
            return;
        }

        var property = text[..colonIndex].Trim();
        var value = text[(colonIndex + 1)..].Trim();

        if (string.IsNullOrEmpty(property) || string.IsNullOrEmpty(value))
        {
            return;
        }

        declarations.Add(new CssDeclaration(property, value));

        // Track custom properties being set
        if (property.StartsWith("--"))
        {
            customProperties.Add(property);
        }
    }

    private void ExtractCustomProperties(string value, HashSet<string> customProperties)
    {
        // Find all var() functions in the value
        var matches = _varFunctionRegex.Matches(value);
        foreach (Match match in matches)
        {
            var propertyName = match.Groups[1].Value.Trim();
            if (propertyName.StartsWith("--"))
            {
                customProperties.Add(propertyName);
            }
        }
    }

    private List<string> SplitUtilities(string applyContent)
    {
        var utilities = new List<string>();
        var current = new StringBuilder();

        for (var i = 0; i < applyContent.Length; i++)
        {
            var c = applyContent[i];

            if (char.IsWhiteSpace(c))
            {
                if (current.Length > 0)
                {
                    utilities.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            utilities.Add(current.ToString());
        }

        return utilities;
    }

    private string RemoveComments(string css)
    {
        var sb = new StringBuilder();
        var inComment = false;
        var inString = false;
        var stringChar = '\0';

        for (var i = 0; i < css.Length; i++)
        {
            var current = css[i];
            var next = i + 1 < css.Length ? css[i + 1] : '\0';

            // Handle strings
            if (!inComment && current is '"' or '\'')
            {
                if (!inString)
                {
                    inString = true;
                    stringChar = current;
                }
                else if (current == stringChar && (i == 0 || css[i - 1] != '\\'))
                {
                    inString = false;
                    stringChar = '\0';
                }

                sb.Append(current);
            }

            // Start of comment
            else if (!inString && !inComment && current == '/' && next == '*')
            {
                inComment = true;
                i++; // Skip the *
            }

            // End of comment
            else if (inComment && current == '*' && next == '/')
            {
                inComment = false;
                i++; // Skip the /
                sb.Append(' '); // Replace comment with space
            }

            // Regular character
            else if (!inComment)
            {
                sb.Append(current);
            }
        }

        return sb.ToString();
    }

    [GeneratedRegex(@"@utility\s+([\w\-\*]+)\s*\{([^}]*(?:\{[^}]*\}[^}]*)*)\}", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex UtilityBlockRegexDefinition();

    [GeneratedRegex(@"var\s*\(\s*([^)]+)\s*\)", RegexOptions.Compiled)]
    private static partial Regex VarFunctionRegexDefinition();

    [GeneratedRegex(@"@apply\s+([^;]+);", RegexOptions.Compiled)]
    private static partial Regex ApplyDirectiveRegexDefinition();
}