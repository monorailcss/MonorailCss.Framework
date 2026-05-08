using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace MonorailCss.Parser.SourceCss;

/// <summary>
/// Parser for <c>@import</c>, <c>@source</c>, <c>@source not</c>, <c>@source inline()</c>, and
/// <c>@custom-variant</c> directives in a Tailwind v4-style CSS source. Returns a structured
/// <see cref="SourceConfiguration"/>; consumers (Discovery, build tasks, the framework's own
/// source-CSS preprocessor) act on the directives.
/// </summary>
public partial class CssSourceParser
{
    private static readonly Regex _sourceRegex = SourceRegexDefinition();
    private static readonly Regex _sourceNotRegex = SourceNotRegexDefinition();
    private static readonly Regex _sourceInlineRegex = SourceInlineRegexDefinition();
    private static readonly Regex _customVariantRegex = CustomVariantRegexDefinition();
    private static readonly Regex _generalImportRegex = GeneralImportRegexDefinition();

    /// <summary>
    /// Parses CSS source and extracts the source configuration.
    /// </summary>
    /// <param name="cssSource">The CSS source to parse.</param>
    /// <returns>The parsed source configuration. Empty when the input is null or whitespace.</returns>
    public SourceConfiguration Parse(string? cssSource)
    {
        if (string.IsNullOrWhiteSpace(cssSource))
        {
            return new SourceConfiguration();
        }

        cssSource = CssCommentStripper.Strip(cssSource);

        string? basePath = null;
        var disableAutoDetection = false;
        var includeSources = new List<SourceDirective>();
        var excludeSources = new List<SourceDirective>();
        var inlineSources = new List<InlineSourceDirective>();
        var customVariants = new List<CustomVariantDefinition>();
        var imports = new List<ImportDirective>();

        ParseAllImportDirectives(cssSource, imports, ref basePath, ref disableAutoDetection);
        ParseSourceDirectives(cssSource, includeSources);
        ParseSourceNotDirectives(cssSource, excludeSources);
        ParseSourceInlineDirectives(cssSource, inlineSources);
        ParseCustomVariantDirectives(cssSource, customVariants);

        return new SourceConfiguration
        {
            BasePath = basePath,
            DisableAutoDetection = disableAutoDetection,
            IncludeSources = includeSources.ToImmutableList(),
            ExcludeSources = excludeSources.ToImmutableList(),
            InlineSources = inlineSources.ToImmutableList(),
            CustomVariants = customVariants.ToImmutableList(),
            Imports = imports.ToImmutableList(),
        };
    }

    private static void ParseAllImportDirectives(
        string cssSource,
        List<ImportDirective> imports,
        ref string? basePath,
        ref bool disableAutoDetection)
    {
        foreach (Match match in _generalImportRegex.Matches(cssSource))
        {
            var path = match.Groups[1].Value.Trim();
            var modifierName = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null;
            var modifierValue = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null;

            var modifier = ImportModifier.None;
            if (!string.IsNullOrEmpty(modifierName))
            {
                modifier = modifierName.ToLowerInvariant() switch
                {
                    "source" => ImportModifier.Source,
                    "theme" => ImportModifier.Theme,
                    "layer" => ImportModifier.Layer,
                    _ => ImportModifier.None,
                };

                if (modifier == ImportModifier.Source && !string.IsNullOrEmpty(modifierValue))
                {
                    if (modifierValue.Equals("none", StringComparison.OrdinalIgnoreCase))
                    {
                        disableAutoDetection = true;
                    }
                    else
                    {
                        basePath = modifierValue.Trim('"', '\'');
                    }
                }
            }

            imports.Add(new ImportDirective
            {
                Path = path,
                Modifier = modifier,
                ModifierValue = modifierValue,
            });
        }
    }

    private static void ParseSourceDirectives(string cssSource, List<SourceDirective> includeSources)
    {
        foreach (Match match in _sourceRegex.Matches(cssSource))
        {
            var path = match.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(path))
            {
                includeSources.Add(new SourceDirective { Path = path });
            }
        }
    }

    private static void ParseSourceNotDirectives(string cssSource, List<SourceDirective> excludeSources)
    {
        foreach (Match match in _sourceNotRegex.Matches(cssSource))
        {
            var path = match.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(path))
            {
                excludeSources.Add(new SourceDirective { Path = path });
            }
        }
    }

    private static void ParseSourceInlineDirectives(string cssSource, List<InlineSourceDirective> inlineSources)
    {
        foreach (Match match in _sourceInlineRegex.Matches(cssSource))
        {
            var pattern = match.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(pattern))
            {
                inlineSources.Add(new InlineSourceDirective
                {
                    Pattern = pattern,
                    ExpandedUtilities = ExpandInlinePattern(pattern),
                });
            }
        }
    }

    private static void ParseCustomVariantDirectives(string cssSource, List<CustomVariantDefinition> customVariants)
    {
        foreach (Match match in _customVariantRegex.Matches(cssSource))
        {
            var name = match.Groups[1].Value.Trim();

            // The regex captures up through the opening '(' so the paren is at the very end of
            // the matched text. We then walk balanced parens manually so selectors like
            // "&:where(.dark, .dark *)" parse correctly.
            var openParenIndex = match.Index + match.Length - 1;
            if (openParenIndex >= cssSource.Length || cssSource[openParenIndex] != '(')
            {
                continue;
            }

            var selector = ExtractBalancedParentheses(cssSource, openParenIndex);
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(selector))
            {
                customVariants.Add(new CustomVariantDefinition
                {
                    Name = name,
                    Selector = selector,
                });
            }
        }
    }

    private static string ExtractBalancedParentheses(string source, int startIndex)
    {
        if (startIndex >= source.Length || source[startIndex] != '(')
        {
            return string.Empty;
        }

        var depth = 0;
        var start = startIndex + 1;
        for (var i = startIndex; i < source.Length; i++)
        {
            var c = source[i];
            if (c == '(')
            {
                depth++;
            }
            else if (c == ')')
            {
                depth--;
                if (depth == 0)
                {
                    return source[start..i].Trim();
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Expands a <c>@source inline()</c> pattern into concrete utility names. Supports brace
    /// lists (<c>bg-red-{50,100,200}</c>), variant groups (<c>{hover:,focus:,}underline</c>), and
    /// numeric ranges (<c>bg-red-{100..900..100}</c>).
    /// </summary>
    /// <param name="pattern">The raw inline-source pattern.</param>
    /// <returns>Expanded utility names; an unrecognised pattern is returned as a single entry.</returns>
    public static ImmutableList<string> ExpandInlinePattern(string pattern)
    {
        var result = new List<string>();

        if (pattern.StartsWith('{') && pattern.Contains('}'))
        {
            var closeBrace = pattern.IndexOf('}');
            var variantGroup = pattern[1..closeBrace];
            var baseUtility = pattern[(closeBrace + 1)..];

            foreach (var variant in variantGroup.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmedVariant = variant.Trim();
                result.Add(string.IsNullOrEmpty(trimmedVariant) ? baseUtility : trimmedVariant + baseUtility);
            }
        }
        else if (pattern.Contains('{') && pattern.Contains('}'))
        {
            var openBrace = pattern.IndexOf('{');
            var closeBrace = pattern.IndexOf('}');
            var prefix = pattern[..openBrace];
            var suffix = pattern[(closeBrace + 1)..];
            var braceContent = pattern[(openBrace + 1)..closeBrace];

            if (braceContent.Contains(".."))
            {
                foreach (var value in ExpandRange(braceContent))
                {
                    result.Add(prefix + value + suffix);
                }
            }
            else
            {
                foreach (var value in braceContent.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    result.Add(prefix + value.Trim() + suffix);
                }
            }
        }
        else
        {
            result.Add(pattern);
        }

        return result.ToImmutableList();
    }

    private static IEnumerable<string> ExpandRange(string rangePattern)
    {
        var parts = rangePattern.Split("..", StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3
            || !int.TryParse(parts[0], out var start)
            || !int.TryParse(parts[1], out var end)
            || !int.TryParse(parts[2], out var step)
            || step == 0)
        {
            yield return rangePattern;
            yield break;
        }

        for (var i = start; step > 0 ? i <= end : i >= end; i += step)
        {
            yield return i.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    [GeneratedRegex("""@import\s+["']([^"']+)["'](?:\s+(source|theme|layer)\s*\(\s*([^)]*)\s*\))?""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex GeneralImportRegexDefinition();

    [GeneratedRegex("""@source\s+(?!not\s|inline\s*\()["']([^"']+)["']""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex SourceRegexDefinition();

    [GeneratedRegex("""@source\s+not\s+["']([^"']+)["']""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex SourceNotRegexDefinition();

    [GeneratedRegex("""@source\s+inline\s*\(\s*["']([^"']+)["']\s*\)""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex SourceInlineRegexDefinition();

    [GeneratedRegex(@"@custom-variant\s+([\w-]+)\s*\(", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex CustomVariantRegexDefinition();
}

/// <summary>
/// Strips C-style block comments (<c>/* … */</c>) from CSS while preserving comment-shaped
/// content inside string literals. Shared helper so theme/source/import parsers all see the
/// same input.
/// </summary>
internal static class CssCommentStripper
{
    public static string Strip(string css)
    {
        var sb = new StringBuilder(css.Length);
        var inComment = false;
        var inString = false;
        var stringChar = '\0';

        for (var i = 0; i < css.Length; i++)
        {
            var current = css[i];
            var next = i + 1 < css.Length ? css[i + 1] : '\0';

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
            else if (!inString && !inComment && current == '/' && next == '*')
            {
                inComment = true;
                i++;
            }
            else if (inComment && current == '*' && next == '/')
            {
                inComment = false;
                i++;
                sb.Append(' ');
            }
            else if (!inComment)
            {
                sb.Append(current);
            }
        }

        return sb.ToString();
    }
}
