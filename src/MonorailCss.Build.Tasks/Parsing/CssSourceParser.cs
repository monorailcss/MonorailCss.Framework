using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace MonorailCss.Build.Tasks.Parsing;

/// <summary>
/// Parser for @source and @import directives in CSS files.
/// Supports Tailwind v4 syntax for controlling source file detection.
/// </summary>
internal partial class CssSourceParser
{
    private static readonly Regex _importRegex = ImportRegexDefinition();
    private static readonly Regex _sourceRegex = SourceRegexDefinition();
    private static readonly Regex _sourceNotRegex = SourceNotRegexDefinition();
    private static readonly Regex _sourceInlineRegex = SourceInlineRegexDefinition();

    /// <summary>
    /// Parses CSS source and extracts source configuration from @import and @source directives.
    /// </summary>
    /// <param name="cssSource">The CSS source to parse.</param>
    /// <returns>The parsed source configuration.</returns>
    public SourceConfiguration Parse(string cssSource)
    {
        if (string.IsNullOrWhiteSpace(cssSource))
        {
            return new SourceConfiguration();
        }

        // Remove comments to simplify parsing
        cssSource = RemoveComments(cssSource);

        string? basePath = null;
        var disableAutoDetection = false;
        var includeSources = new List<SourceDirective>();
        var excludeSources = new List<SourceDirective>();
        var inlineSources = new List<InlineSourceDirective>();

        // Parse @import directives with source() function
        ParseImportDirectives(cssSource, ref basePath, ref disableAutoDetection);

        // Parse @source directives
        ParseSourceDirectives(cssSource, includeSources);

        // Parse @source not directives
        ParseSourceNotDirectives(cssSource, excludeSources);

        // Parse @source inline() directives
        ParseSourceInlineDirectives(cssSource, inlineSources);

        return new SourceConfiguration
        {
            BasePath = basePath,
            DisableAutoDetection = disableAutoDetection,
            IncludeSources = includeSources.ToImmutableList(),
            ExcludeSources = excludeSources.ToImmutableList(),
            InlineSources = inlineSources.ToImmutableList()
        };
    }

    private void ParseImportDirectives(string cssSource, ref string? basePath, ref bool disableAutoDetection)
    {
        var matches = _importRegex.Matches(cssSource);

        foreach (Match match in matches)
        {
            var sourceParam = match.Groups[1].Value.Trim();

            // Check for source(none)
            if (sourceParam.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                disableAutoDetection = true;
            }
            // Extract path from quotes
            else if (sourceParam.StartsWith('"') && sourceParam.EndsWith('"'))
            {
                basePath = sourceParam.Trim('"');
            }
            else if (sourceParam.StartsWith('\'') && sourceParam.EndsWith('\''))
            {
                basePath = sourceParam.Trim('\'');
            }
        }
    }

    private void ParseSourceDirectives(string cssSource, List<SourceDirective> includeSources)
    {
        var matches = _sourceRegex.Matches(cssSource);

        foreach (Match match in matches)
        {
            var path = match.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(path))
            {
                includeSources.Add(new SourceDirective { Path = path });
            }
        }
    }

    private void ParseSourceNotDirectives(string cssSource, List<SourceDirective> excludeSources)
    {
        var matches = _sourceNotRegex.Matches(cssSource);

        foreach (Match match in matches)
        {
            var path = match.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(path))
            {
                excludeSources.Add(new SourceDirective { Path = path });
            }
        }
    }

    private void ParseSourceInlineDirectives(string cssSource, List<InlineSourceDirective> inlineSources)
    {
        var matches = _sourceInlineRegex.Matches(cssSource);

        foreach (Match match in matches)
        {
            var pattern = match.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(pattern))
            {
                // Expand the pattern into individual utilities
                var expanded = ExpandInlinePattern(pattern);
                inlineSources.Add(new InlineSourceDirective
                {
                    Pattern = pattern,
                    ExpandedUtilities = expanded
                });
            }
        }
    }

    /// <summary>
    /// Expands inline patterns that may contain brace expansion or variant groups.
    /// Examples:
    /// - "underline" -> ["underline"]
    /// - "bg-red-{50,100,200}" -> ["bg-red-50", "bg-red-100", "bg-red-200"]
    /// - "{hover:,focus:,}underline" -> ["underline", "hover:underline", "focus:underline"]
    /// - "bg-red-{100..900..100}" -> ["bg-red-100", "bg-red-200", ..., "bg-red-900"]
    /// </summary>
    private ImmutableList<string> ExpandInlinePattern(string pattern)
    {
        var result = new List<string>();

        // Handle variant group patterns like "{hover:,focus:,}utility"
        if (pattern.StartsWith('{') && pattern.Contains('}'))
        {
            var closeBrace = pattern.IndexOf('}');
            var variantGroup = pattern[1..closeBrace];
            var baseUtility = pattern[(closeBrace + 1)..];

            var variants = variantGroup.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var variant in variants)
            {
                var trimmedVariant = variant.Trim();
                if (string.IsNullOrEmpty(trimmedVariant))
                {
                    // Empty variant means no prefix
                    result.Add(baseUtility);
                }
                else
                {
                    result.Add(trimmedVariant + baseUtility);
                }
            }
        }
        // Handle brace expansion in the middle like "bg-red-{50,100,200}"
        else if (pattern.Contains('{') && pattern.Contains('}'))
        {
            var openBrace = pattern.IndexOf('{');
            var closeBrace = pattern.IndexOf('}');
            var prefix = pattern[..openBrace];
            var suffix = pattern[(closeBrace + 1)..];
            var braceContent = pattern[(openBrace + 1)..closeBrace];

            // Check for range syntax like "100..900..100"
            if (braceContent.Contains(".."))
            {
                var expandedRange = ExpandRange(braceContent);
                foreach (var value in expandedRange)
                {
                    result.Add(prefix + value + suffix);
                }
            }
            else
            {
                // Simple comma-separated list
                var values = braceContent.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in values)
                {
                    var trimmedValue = value.Trim();
                    result.Add(prefix + trimmedValue + suffix);
                }
            }
        }
        else
        {
            // No expansion needed
            result.Add(pattern);
        }

        return result.ToImmutableList();
    }

    /// <summary>
    /// Expands a range pattern like "100..900..100" into ["100", "200", "300", ..., "900"].
    /// </summary>
    private List<string> ExpandRange(string rangePattern)
    {
        var parts = rangePattern.Split("..", StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            // Invalid range format, return as-is
            return [rangePattern];
        }

        if (!int.TryParse(parts[0], out var start) ||
            !int.TryParse(parts[1], out var end) ||
            !int.TryParse(parts[2], out var step))
        {
            // Invalid numbers, return as-is
            return [rangePattern];
        }

        var result = new List<string>();
        for (var i = start; i <= end; i += step)
        {
            result.Add(i.ToString());
        }

        return result;
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

    // Matches @import "tailwindcss" source(...)
    [GeneratedRegex(@"@import\s+[""']tailwindcss[""']\s+source\s*\(\s*([^)]+)\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex ImportRegexDefinition();

    // Matches @source "path" (but not @source not or @source inline)
    [GeneratedRegex(@"@source\s+(?!not\s|inline\s*\()[""']([^""']+)[""']", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex SourceRegexDefinition();

    // Matches @source not "path"
    [GeneratedRegex(@"@source\s+not\s+[""']([^""']+)[""']", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex SourceNotRegexDefinition();

    // Matches @source inline("pattern")
    [GeneratedRegex(@"@source\s+inline\s*\(\s*[""']([^""']+)[""']\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex SourceInlineRegexDefinition();
}
