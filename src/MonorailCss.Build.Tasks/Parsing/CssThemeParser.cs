using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace MonorailCss.Build.Tasks.Parsing;

/// <summary>
/// Represents a parsed custom utility definition.
/// </summary>
public record ParsedUtilityDefinition(
    string Pattern,
    bool IsWildcard,
    ImmutableList<ParsedCssDeclaration> Declarations,
    ImmutableList<ParsedNestedSelector> NestedSelectors,
    ImmutableList<string> CustomPropertyDependencies);

/// <summary>
/// Represents a CSS declaration.
/// </summary>
public record ParsedCssDeclaration(string Property, string Value);

/// <summary>
/// Represents a nested selector.
/// </summary>
public record ParsedNestedSelector(string Selector, ImmutableList<ParsedCssDeclaration> Declarations);

/// <summary>
/// Parses CSS source containing @theme blocks and component rules with @apply directives.
/// Supports a minimal subset of Tailwind v4 syntax for IntelliSense compatibility.
/// This version is decoupled from the main framework and returns DTOs.
/// </summary>
internal partial class CssThemeParser
{
    private static readonly Regex _importRegex = ImportRegexDefinition();
    private static readonly Regex _themeBlockRegex = ThemeBlockRegexDefinition();
    private static readonly Regex _themeVariableRegex = ThemeVariableRegexDefinition();
    private static readonly Regex _componentRuleRegex = ComponentRuleRegexDefinition();
    private static readonly Regex _applyDirectiveRegex = ApplyDirectiveRegexDefinition();

    /// <summary>
    /// Result of parsing CSS source.
    /// </summary>
    public record ParseResult(
        ImmutableDictionary<string, string> ThemeVariables,
        ImmutableDictionary<string, string> ComponentRules,
        ImmutableList<ParsedUtilityDefinition> UtilityDefinitions);

    /// <summary>
    /// Parses CSS source and extracts theme variables and component rules.
    /// </summary>
    /// <param name="cssSource">The CSS source string to parse.</param>
    /// <returns>Parsed theme variables and component rules.</returns>
    public ParseResult Parse(string cssSource)
    {
        if (string.IsNullOrWhiteSpace(cssSource))
        {
            return new ParseResult(
                ImmutableDictionary<string, string>.Empty,
                ImmutableDictionary<string, string>.Empty,
                ImmutableList<ParsedUtilityDefinition>.Empty);
        }

        // Remove comments to simplify parsing
        cssSource = RemoveComments(cssSource);

        // Check for @import "tailwindcss"
        var hasImport = _importRegex.IsMatch(cssSource);

        // Extract theme variables from @theme blocks
        var themeVariables = ExtractThemeVariables(cssSource);

        // Extract component rules with @apply directives
        var componentRules = ExtractComponentRules(cssSource);

        // Extract custom utilities from @utility blocks
        var utilities = ExtractUtilities(cssSource);

        return new ParseResult(themeVariables, componentRules, utilities);
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

    private ImmutableDictionary<string, string> ExtractThemeVariables(string css)
    {
        var variables = ImmutableDictionary.CreateBuilder<string, string>();

        var themeMatches = _themeBlockRegex.Matches(css);
        foreach (Match themeMatch in themeMatches)
        {
            var themeContent = themeMatch.Groups[1].Value;

            var variableMatches = _themeVariableRegex.Matches(themeContent);
            foreach (Match varMatch in variableMatches)
            {
                var varName = varMatch.Groups[1].Value.Trim();
                var varValue = varMatch.Groups[2].Value.Trim();

                // Store the variable (last one wins if duplicates)
                variables[varName] = varValue;
            }
        }

        return variables.ToImmutable();
    }

    private ImmutableDictionary<string, string> ExtractComponentRules(string css)
    {
        var components = ImmutableDictionary.CreateBuilder<string, string>();

        // Remove @theme blocks to avoid parsing them as component rules
        css = _themeBlockRegex.Replace(css, string.Empty);

        var componentMatches = _componentRuleRegex.Matches(css);
        foreach (Match componentMatch in componentMatches)
        {
            var selector = componentMatch.Groups[1].Value.Trim();
            var ruleContent = componentMatch.Groups[2].Value;

            // Extract @apply directives from the rule
            var applyMatches = _applyDirectiveRegex.Matches(ruleContent);
            if (applyMatches.Count > 0)
            {
                var utilities = new List<string>();
                foreach (Match applyMatch in applyMatches)
                {
                    var applyContent = applyMatch.Groups[1].Value.Trim();

                    // Split by whitespace but preserve variant: prefixes
                    utilities.AddRange(SplitUtilities(applyContent));
                }

                if (utilities.Count > 0)
                {
                    // Store the component rule (last one wins if duplicates)
                    components[selector] = string.Join(" ", utilities);
                }
            }
        }

        return components.ToImmutable();
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

    private ImmutableList<ParsedUtilityDefinition> ExtractUtilities(string css)
    {
        // Use the custom utility parser to extract utility definitions
        var parser = new CustomUtilityCssParser();
        var definitions = parser.ParseCustomUtilities(css);

        // Convert to parsed utility definitions (DTOs)
        var utilities = definitions.Select(def => new ParsedUtilityDefinition(
            def.Pattern,
            def.IsWildcard,
            def.Declarations.Select(d => new ParsedCssDeclaration(d.Property, d.Value)).ToImmutableList(),
            def.NestedSelectors.Select(ns => new ParsedNestedSelector(
                ns.Selector,
                ns.Declarations.Select(d => new ParsedCssDeclaration(d.Property, d.Value)).ToImmutableList()
            )).ToImmutableList(),
            def.CustomPropertyDependencies
        )).ToImmutableList();

        return utilities;
    }

    [GeneratedRegex(@"@import\s+[""']tailwindcss[""']\s*;?", RegexOptions.Compiled)]
    private static partial Regex ImportRegexDefinition();
    [GeneratedRegex(@"@theme\s*\{([^}]*)\}", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex ThemeBlockRegexDefinition();
    [GeneratedRegex(@"(--([\w-]+))\s*:\s*([^;]+);", RegexOptions.Compiled)]
    private static partial Regex ThemeVariableRegexDefinition();
    [GeneratedRegex(@"([\w\s\-\.\#\:\[\]]+)\s*\{([^}]*@apply[^}]*)\}", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex ComponentRuleRegexDefinition();
    [GeneratedRegex(@"@apply\s+([^;]+);", RegexOptions.Compiled)]
    private static partial Regex ApplyDirectiveRegexDefinition();
}