using System.Collections.Immutable;
using System.Text.RegularExpressions;
using MonorailCss.Parser.Custom;

namespace MonorailCss.Parser.SourceCss;

/// <summary>
/// Result of parsing a Tailwind v4-style CSS source for theme variables, component rules,
/// custom utilities, and source configuration.
/// </summary>
/// <param name="ThemeVariables">Variables from <c>@theme {…}</c> blocks.</param>
/// <param name="InlineThemeVariables">Variables from <c>@theme inline {…}</c> blocks (values
/// resolved through any <c>var()</c> references known to the running framework).</param>
/// <param name="StaticThemeVariables">Variables from <c>@theme static {…}</c> blocks.</param>
/// <param name="StaticInlineThemeVariables">Variables from <c>@theme static inline {…}</c> blocks.</param>
/// <param name="ComponentRules">Selectors paired with their concatenated <c>@apply</c> utility
/// list, suitable for assignment to <see cref="CssFrameworkSettings.Applies"/>.</param>
/// <param name="UtilityDefinitions">Definitions parsed from <c>@utility</c> blocks.</param>
/// <param name="SourceConfiguration">Configuration extracted from <c>@import</c>/<c>@source</c>/<c>@custom-variant</c>.</param>
public record CssThemeParseResult(
    ImmutableDictionary<string, string> ThemeVariables,
    ImmutableDictionary<string, string> InlineThemeVariables,
    ImmutableDictionary<string, string> StaticThemeVariables,
    ImmutableDictionary<string, string> StaticInlineThemeVariables,
    ImmutableDictionary<string, string> ComponentRules,
    ImmutableList<UtilityDefinition> UtilityDefinitions,
    SourceConfiguration SourceConfiguration)
{
    /// <summary>Empty parse result.</summary>
    public static readonly CssThemeParseResult Empty = new(
        ImmutableDictionary<string, string>.Empty,
        ImmutableDictionary<string, string>.Empty,
        ImmutableDictionary<string, string>.Empty,
        ImmutableDictionary<string, string>.Empty,
        ImmutableDictionary<string, string>.Empty,
        ImmutableList<UtilityDefinition>.Empty,
        new SourceConfiguration());
}

/// <summary>
/// Parses a Tailwind v4-style CSS source for the directives the framework's settings care about:
/// <c>@theme</c> blocks (with all four <c>static</c>/<c>inline</c> modifier combinations), component
/// selectors carrying <c>@apply</c>, <c>@utility</c> blocks, and the directives covered by
/// <see cref="CssSourceParser"/>.
/// </summary>
public partial class CssThemeParser
{
    private static readonly Regex _themeVariableRegex = ThemeVariableRegexDefinition();
    private static readonly Regex _componentRuleRegex = ComponentRuleRegexDefinition();
    private static readonly Regex _applyDirectiveRegex = ApplyDirectiveRegexDefinition();

    /// <summary>
    /// Parses CSS source.
    /// </summary>
    /// <param name="cssSource">The CSS source.</param>
    /// <returns>The parse result; <see cref="CssThemeParseResult.Empty"/> when input is null/whitespace.</returns>
    public CssThemeParseResult Parse(string? cssSource)
    {
        if (string.IsNullOrWhiteSpace(cssSource))
        {
            return CssThemeParseResult.Empty;
        }

        cssSource = CssCommentStripper.Strip(cssSource);

        var (regular, inline, staticVars, staticInline) = ExtractThemeVariables(cssSource);

        var componentRules = ExtractComponentRules(cssSource);

        var utilityParser = new CustomUtilityCssParser();
        var utilityDefinitions = utilityParser.ParseCustomUtilities(cssSource).ToImmutableList();

        var sourceConfiguration = new CssSourceParser().Parse(cssSource);

        return new CssThemeParseResult(
            regular,
            inline,
            staticVars,
            staticInline,
            componentRules,
            utilityDefinitions,
            sourceConfiguration);
    }

    private static (
        ImmutableDictionary<string, string> Regular,
        ImmutableDictionary<string, string> Inline,
        ImmutableDictionary<string, string> Static,
        ImmutableDictionary<string, string> StaticInline) ExtractThemeVariables(string css)
    {
        var regular = ImmutableDictionary.CreateBuilder<string, string>();
        var inline = ImmutableDictionary.CreateBuilder<string, string>();
        var staticVars = ImmutableDictionary.CreateBuilder<string, string>();
        var staticInline = ImmutableDictionary.CreateBuilder<string, string>();

        var i = 0;
        while (i < css.Length)
        {
            var themeIndex = css.IndexOf("@theme", i, StringComparison.OrdinalIgnoreCase);
            if (themeIndex == -1)
            {
                break;
            }

            var modifiersStart = themeIndex + "@theme".Length;
            var openBrace = css.IndexOf('{', modifiersStart);
            if (openBrace == -1)
            {
                break;
            }

            var modifiers = css.Substring(modifiersStart, openBrace - modifiersStart);
            var isStatic = ContainsKeyword(modifiers, "static");
            var isInline = ContainsKeyword(modifiers, "inline");

            // Walk balanced braces so nested @keyframes / @utility blocks inside @theme don't
            // terminate the scan early.
            var depth = 1;
            var contentStart = openBrace + 1;
            var j = contentStart;
            while (j < css.Length && depth > 0)
            {
                if (css[j] == '{')
                {
                    depth++;
                }
                else if (css[j] == '}')
                {
                    depth--;
                }

                j++;
            }

            if (depth != 0)
            {
                break;
            }

            var content = css.Substring(contentStart, j - contentStart - 1);
            var target = (isStatic, isInline) switch
            {
                (true, true) => staticInline,
                (true, false) => staticVars,
                (false, true) => inline,
                (false, false) => regular,
            };

            ExtractVariablesFromContent(content, target);

            i = j;
        }

        return (regular.ToImmutable(), inline.ToImmutable(), staticVars.ToImmutable(), staticInline.ToImmutable());
    }

    private static bool ContainsKeyword(string modifiers, string keyword)
    {
        // Word-boundary check so "inline-foo" wouldn't be treated as the inline modifier.
        var idx = modifiers.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return false;
        }

        var before = idx == 0 || char.IsWhiteSpace(modifiers[idx - 1]);
        var after = idx + keyword.Length >= modifiers.Length
            || char.IsWhiteSpace(modifiers[idx + keyword.Length]);
        return before && after;
    }

    private static void ExtractVariablesFromContent(string content, ImmutableDictionary<string, string>.Builder target)
    {
        // Strip nested @keyframes / @utility blocks so their inner declarations don't leak in.
        content = StripNestedAtBlocks(content, "@keyframes");
        content = StripNestedAtBlocks(content, "@utility");

        foreach (Match match in _themeVariableRegex.Matches(content))
        {
            var name = match.Groups[1].Value.Trim();
            var value = match.Groups[2].Value.Trim();
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
            {
                target[name] = value;
            }
        }
    }

    private static string StripNestedAtBlocks(string body, string atKeyword)
    {
        var sb = new System.Text.StringBuilder(body.Length);
        var i = 0;
        while (i < body.Length)
        {
            var idx = body.IndexOf(atKeyword, i, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                sb.Append(body, i, body.Length - i);
                break;
            }

            sb.Append(body, i, idx - i);

            var open = body.IndexOf('{', idx + atKeyword.Length);
            if (open < 0)
            {
                break;
            }

            var depth = 1;
            var j = open + 1;
            while (j < body.Length && depth > 0)
            {
                if (body[j] == '{')
                {
                    depth++;
                }
                else if (body[j] == '}')
                {
                    depth--;
                }

                j++;
            }

            i = j;
        }

        return sb.ToString();
    }

    private static ImmutableDictionary<string, string> ExtractComponentRules(string css)
    {
        var components = ImmutableDictionary.CreateBuilder<string, string>();

        // Skip over @theme/@utility blocks before scanning for component rules. We can't use a
        // simple regex strip because those blocks may contain unbalanced-looking inner content
        // when comments are stripped naively.
        css = StripNestedAtBlocks(css, "@theme");
        css = StripNestedAtBlocks(css, "@utility");

        // Unwrap `@layer <name> { … }` so component selectors inside a layer are visible.
        // We do this by replacing the layer wrapper with its inner content; nested layers
        // still work because the regex re-runs.
        var changed = true;
        while (changed)
        {
            changed = false;
            var idx = 0;
            var sb = new System.Text.StringBuilder(css.Length);
            while (idx < css.Length)
            {
                var match = css.IndexOf("@layer", idx, StringComparison.OrdinalIgnoreCase);
                if (match < 0)
                {
                    sb.Append(css, idx, css.Length - idx);
                    break;
                }

                sb.Append(css, idx, match - idx);
                var open = css.IndexOf('{', match + "@layer".Length);
                if (open < 0)
                {
                    sb.Append(css, match, css.Length - match);
                    break;
                }

                var depth = 1;
                var j = open + 1;
                while (j < css.Length && depth > 0)
                {
                    if (css[j] == '{')
                    {
                        depth++;
                    }
                    else if (css[j] == '}')
                    {
                        depth--;
                    }

                    j++;
                }

                if (depth != 0)
                {
                    sb.Append(css, match, css.Length - match);
                    break;
                }

                // Append the inner content (without the @layer wrapper or its name).
                sb.Append(' ');
                sb.Append(css, open + 1, j - open - 2);
                sb.Append(' ');
                idx = j;
                changed = true;
            }

            css = sb.ToString();
        }

        foreach (Match match in _componentRuleRegex.Matches(css))
        {
            var selector = match.Groups[1].Value.Trim();
            var ruleContent = match.Groups[2].Value;

            var utilities = new List<string>();
            foreach (Match applyMatch in _applyDirectiveRegex.Matches(ruleContent))
            {
                foreach (var token in applyMatch.Groups[1].Value.Split(' ', '\t', '\r', '\n'))
                {
                    var trimmed = token.Trim();
                    if (trimmed.Length > 0)
                    {
                        utilities.Add(trimmed);
                    }
                }
            }

            if (utilities.Count > 0 && selector.Length > 0)
            {
                components[selector] = string.Join(" ", utilities);
            }
        }

        return components.ToImmutable();
    }

    [GeneratedRegex(@"(--[\w-]+)\s*:\s*([^;]+);", RegexOptions.Compiled)]
    private static partial Regex ThemeVariableRegexDefinition();

    [GeneratedRegex(@"([\w\s\-\.\#\:\[\]]+)\s*\{([^}]*@apply[^}]*)\}", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex ComponentRuleRegexDefinition();

    [GeneratedRegex(@"@apply\s+([^;]+);", RegexOptions.Compiled)]
    private static partial Regex ApplyDirectiveRegexDefinition();
}
