using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using MonorailCss.Parser.Custom;

namespace MonorailCss.Parser.SourceCss;

/// <summary>
/// Result of recursively processing a CSS source file and its <c>@import</c>s. Aggregates
/// theme variables, component rules, custom utilities, and source configuration across the
/// whole import graph and preserves any pass-through CSS rules (font-faces, keyframes, plain
/// selectors) for verbatim emission alongside the framework-generated output.
/// </summary>
/// <param name="ThemeVariables">Variables from <c>@theme {…}</c> blocks across all imported files.</param>
/// <param name="InlineThemeVariables">Variables from <c>@theme inline {…}</c> blocks.</param>
/// <param name="StaticThemeVariables">Variables from <c>@theme static {…}</c> blocks.</param>
/// <param name="StaticInlineThemeVariables">Variables from <c>@theme static inline {…}</c> blocks.</param>
/// <param name="ComponentRules">Selector → <c>@apply</c> map suitable for <see cref="CssFrameworkSettings.Applies"/>.</param>
/// <param name="UtilityDefinitions">Definitions parsed from <c>@utility</c> blocks across all files.</param>
/// <param name="SourceConfiguration">Aggregated <c>@import</c>/<c>@source</c>/<c>@custom-variant</c> directives.</param>
/// <param name="RawCssRules">CSS the parser didn't consume; emit verbatim alongside the generated output.</param>
/// <param name="ImportedFiles">Absolute paths of every file pulled into the graph (in import order).</param>
/// <param name="Keyframes">Animation name → keyframes body extracted from <c>@keyframes</c>
/// blocks declared inside any <c>@theme</c> block. Later imports override earlier ones on
/// name collision.</param>
public record CssImportResult(
    ImmutableDictionary<string, string> ThemeVariables,
    ImmutableDictionary<string, string> InlineThemeVariables,
    ImmutableDictionary<string, string> StaticThemeVariables,
    ImmutableDictionary<string, string> StaticInlineThemeVariables,
    ImmutableDictionary<string, string> ComponentRules,
    ImmutableList<UtilityDefinition> UtilityDefinitions,
    SourceConfiguration SourceConfiguration,
    ImmutableList<RawCssRule> RawCssRules,
    ImmutableList<string> ImportedFiles,
    ImmutableDictionary<string, string> Keyframes)
{
    /// <summary>
    /// Empty result for null/missing inputs.
    /// </summary>
    public static readonly CssImportResult Empty = new(
        ImmutableDictionary<string, string>.Empty,
        ImmutableDictionary<string, string>.Empty,
        ImmutableDictionary<string, string>.Empty,
        ImmutableDictionary<string, string>.Empty,
        ImmutableDictionary<string, string>.Empty,
        ImmutableList<UtilityDefinition>.Empty,
        new SourceConfiguration(),
        ImmutableList<RawCssRule>.Empty,
        ImmutableList<string>.Empty,
        ImmutableDictionary<string, string>.Empty);
}

/// <summary>
/// A pass-through CSS rule emitted verbatim alongside the framework-generated output.
/// </summary>
/// <param name="Selector">The selector (empty when <see cref="Content"/> is whole-file residue).</param>
/// <param name="Content">The CSS content.</param>
/// <param name="SourceFile">The file the content came from (absolute path).</param>
public record RawCssRule(string Selector, string Content, string SourceFile);

/// <summary>
/// Recursively processes a CSS source file, following <c>@import</c> directives, and merges
/// the parsed contributions of every visited file into a <see cref="CssImportResult"/>.
/// </summary>
/// <remarks>
/// <para>
/// File access is direct <see cref="System.IO"/>; logging is via an optional callback.
/// </para>
/// <para>
/// Special-case: <c>@import "tailwindcss"</c> is treated as the framework's own import (no
/// physical file on disk) and is not followed.
/// </para>
/// </remarks>
public partial class CssImportProcessor
{
    private readonly CssThemeParser _themeParser = new();
    private readonly HashSet<string> _processedFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly Action<string>? _log;

    /// <summary>
    /// Initializes a new instance of the <see cref="CssImportProcessor"/> class.
    /// </summary>
    /// <param name="log">Optional log callback invoked with progress / warning messages.</param>
    public CssImportProcessor(Action<string>? log = null)
    {
        _log = log;
    }

    /// <summary>
    /// Processes the given CSS file and every file it transitively imports.
    /// </summary>
    /// <param name="cssFilePath">Path to the entry file. Relative paths resolve against the current working directory.</param>
    /// <returns>The merged result; <see cref="CssImportResult.Empty"/> when the file is missing.</returns>
    public CssImportResult ProcessImports(string cssFilePath)
    {
        _processedFiles.Clear();

        var theme = ImmutableDictionary.CreateBuilder<string, string>();
        var inline = ImmutableDictionary.CreateBuilder<string, string>();
        var staticVars = ImmutableDictionary.CreateBuilder<string, string>();
        var staticInline = ImmutableDictionary.CreateBuilder<string, string>();
        var components = ImmutableDictionary.CreateBuilder<string, string>();
        var utilities = ImmutableList.CreateBuilder<UtilityDefinition>();
        var raw = ImmutableList.CreateBuilder<RawCssRule>();
        var imported = ImmutableList.CreateBuilder<string>();
        var keyframes = ImmutableDictionary.CreateBuilder<string, string>();
        var sourceConfiguration = new SourceConfiguration();

        ProcessFileRecursive(
            cssFilePath,
            theme,
            inline,
            staticVars,
            staticInline,
            components,
            utilities,
            raw,
            imported,
            keyframes,
            ref sourceConfiguration);

        return new CssImportResult(
            theme.ToImmutable(),
            inline.ToImmutable(),
            staticVars.ToImmutable(),
            staticInline.ToImmutable(),
            components.ToImmutable(),
            utilities.ToImmutable(),
            sourceConfiguration,
            raw.ToImmutable(),
            imported.ToImmutable(),
            keyframes.ToImmutable());
    }

    /// <summary>
    /// Processes an in-memory CSS string. <c>@import</c> directives that reference files on
    /// disk are followed when <paramref name="basePath"/> is provided; otherwise they are
    /// recorded in the result's <see cref="SourceConfiguration.Imports"/> but not loaded.
    /// </summary>
    /// <param name="cssSource">The CSS source.</param>
    /// <param name="basePath">Directory used to resolve relative <c>@import</c> paths.</param>
    /// <returns>The merged result.</returns>
    public CssImportResult ProcessSource(string cssSource, string? basePath = null)
    {
        _processedFiles.Clear();

        var theme = ImmutableDictionary.CreateBuilder<string, string>();
        var inline = ImmutableDictionary.CreateBuilder<string, string>();
        var staticVars = ImmutableDictionary.CreateBuilder<string, string>();
        var staticInline = ImmutableDictionary.CreateBuilder<string, string>();
        var components = ImmutableDictionary.CreateBuilder<string, string>();
        var utilities = ImmutableList.CreateBuilder<UtilityDefinition>();
        var raw = ImmutableList.CreateBuilder<RawCssRule>();
        var imported = ImmutableList.CreateBuilder<string>();
        var keyframes = ImmutableDictionary.CreateBuilder<string, string>();
        var sourceConfiguration = new SourceConfiguration();

        ApplyParsedSource(
            cssSource,
            sourceFileLabel: "<inline>",
            theme,
            inline,
            staticVars,
            staticInline,
            components,
            utilities,
            raw,
            keyframes,
            ref sourceConfiguration);

        if (!string.IsNullOrEmpty(basePath))
        {
            foreach (var import in sourceConfiguration.Imports)
            {
                if (string.Equals(import.Path, "tailwindcss", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var importPath = ResolveImportPath(import.Path, basePath);
                ProcessFileRecursive(
                    importPath,
                    theme,
                    inline,
                    staticVars,
                    staticInline,
                    components,
                    utilities,
                    raw,
                    imported,
                    keyframes,
                    ref sourceConfiguration);
            }
        }

        return new CssImportResult(
            theme.ToImmutable(),
            inline.ToImmutable(),
            staticVars.ToImmutable(),
            staticInline.ToImmutable(),
            components.ToImmutable(),
            utilities.ToImmutable(),
            sourceConfiguration,
            raw.ToImmutable(),
            imported.ToImmutable(),
            keyframes.ToImmutable());
    }

    private void ProcessFileRecursive(
        string cssFilePath,
        ImmutableDictionary<string, string>.Builder theme,
        ImmutableDictionary<string, string>.Builder inline,
        ImmutableDictionary<string, string>.Builder staticVars,
        ImmutableDictionary<string, string>.Builder staticInline,
        ImmutableDictionary<string, string>.Builder components,
        ImmutableList<UtilityDefinition>.Builder utilities,
        ImmutableList<RawCssRule>.Builder raw,
        ImmutableList<string>.Builder imported,
        ImmutableDictionary<string, string>.Builder keyframes,
        ref SourceConfiguration sourceConfiguration)
    {
        var normalized = Path.GetFullPath(cssFilePath);
        if (_processedFiles.Contains(normalized))
        {
            _log?.Invoke($"Skipping already-processed file: {normalized}");
            return;
        }

        if (!File.Exists(normalized))
        {
            _log?.Invoke($"CSS import not found: {normalized}");
            return;
        }

        _processedFiles.Add(normalized);
        imported.Add(normalized);
        _log?.Invoke($"Processing CSS file: {normalized}");

        var content = File.ReadAllText(normalized);
        ApplyParsedSource(
            content,
            sourceFileLabel: normalized,
            theme,
            inline,
            staticVars,
            staticInline,
            components,
            utilities,
            raw,
            keyframes,
            ref sourceConfiguration);

        var directory = Path.GetDirectoryName(normalized) ?? string.Empty;

        // Only follow imports declared in this file (not the global accumulated set), so we
        // resolve relative to the file actually containing each `@import`.
        var fileImports = new CssSourceParser().Parse(content).Imports;
        foreach (var import in fileImports)
        {
            if (string.Equals(import.Path, "tailwindcss", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var importPath = ResolveImportPath(import.Path, directory);
            ProcessFileRecursive(
                importPath,
                theme,
                inline,
                staticVars,
                staticInline,
                components,
                utilities,
                raw,
                imported,
                keyframes,
                ref sourceConfiguration);
        }
    }

    private void ApplyParsedSource(
        string content,
        string sourceFileLabel,
        ImmutableDictionary<string, string>.Builder theme,
        ImmutableDictionary<string, string>.Builder inline,
        ImmutableDictionary<string, string>.Builder staticVars,
        ImmutableDictionary<string, string>.Builder staticInline,
        ImmutableDictionary<string, string>.Builder components,
        ImmutableList<UtilityDefinition>.Builder utilities,
        ImmutableList<RawCssRule>.Builder raw,
        ImmutableDictionary<string, string>.Builder keyframes,
        ref SourceConfiguration sourceConfiguration)
    {
        var parseResult = _themeParser.Parse(content);

        foreach (var (k, v) in parseResult.ThemeVariables)
        {
            theme[k] = v;
        }

        foreach (var (k, v) in parseResult.InlineThemeVariables)
        {
            inline[k] = v;
        }

        foreach (var (k, v) in parseResult.StaticThemeVariables)
        {
            staticVars[k] = v;
        }

        foreach (var (k, v) in parseResult.StaticInlineThemeVariables)
        {
            staticInline[k] = v;
        }

        foreach (var (k, v) in parseResult.ComponentRules)
        {
            components[k] = v;
        }

        // Last-write-wins on name collision so a later @import can override an earlier
        // keyframes definition, mirroring how variables and applies behave.
        foreach (var (name, body) in parseResult.Keyframes)
        {
            keyframes[name] = body;
        }

        utilities.AddRange(parseResult.UtilityDefinitions);

        sourceConfiguration = MergeSourceConfiguration(sourceConfiguration, parseResult.SourceConfiguration);

        ExtractRawCss(content, sourceFileLabel, raw);
    }

    private static SourceConfiguration MergeSourceConfiguration(SourceConfiguration current, SourceConfiguration incoming)
    {
        return current with
        {
            BasePath = current.BasePath ?? incoming.BasePath,
            DisableAutoDetection = current.DisableAutoDetection || incoming.DisableAutoDetection,
            IncludeSources = current.IncludeSources.AddRange(incoming.IncludeSources),
            ExcludeSources = current.ExcludeSources.AddRange(incoming.ExcludeSources),
            InlineSources = current.InlineSources.AddRange(incoming.InlineSources),
            CustomVariants = current.CustomVariants.AddRange(incoming.CustomVariants),
            Imports = current.Imports.AddRange(incoming.Imports),
        };
    }

    private static string ResolveImportPath(string importPath, string baseDirectory)
    {
        var clean = importPath.Trim('"', '\'');
        if (!Path.IsPathRooted(clean))
        {
            clean = Path.Combine(baseDirectory, clean);
        }

        if (!File.Exists(clean) && !clean.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
        {
            var withCss = clean + ".css";
            if (File.Exists(withCss))
            {
                return Path.GetFullPath(withCss);
            }
        }

        return Path.GetFullPath(clean);
    }

    private void ExtractRawCss(string content, string sourceFile, ImmutableList<RawCssRule>.Builder rules)
    {
        var stripped = CssCommentStripper.Strip(content);
        stripped = StripBlock(stripped, "@theme");
        stripped = StripBlock(stripped, "@utility");
        stripped = SourceDirectiveRegex().Replace(stripped, string.Empty);
        stripped = ImportDirectiveRegex().Replace(stripped, string.Empty);
        stripped = CustomVariantDirectiveRegex().Replace(stripped, string.Empty);

        var trimmed = stripped.Trim();
        if (trimmed.Length > 0)
        {
            rules.Add(new RawCssRule(Selector: string.Empty, Content: trimmed, SourceFile: sourceFile));
        }
    }

    private static string StripBlock(string css, string atKeyword)
    {
        var sb = new StringBuilder(css.Length);
        var i = 0;
        while (i < css.Length)
        {
            var idx = css.IndexOf(atKeyword, i, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                sb.Append(css, i, css.Length - i);
                break;
            }

            sb.Append(css, i, idx - i);

            var open = css.IndexOf('{', idx + atKeyword.Length);
            if (open < 0)
            {
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

            i = j;
        }

        return sb.ToString();
    }

    [GeneratedRegex(@"@source\s+[^;]+;", RegexOptions.IgnoreCase)]
    private static partial Regex SourceDirectiveRegex();

    [GeneratedRegex(@"@import\s+[^;]+;", RegexOptions.IgnoreCase)]
    private static partial Regex ImportDirectiveRegex();

    [GeneratedRegex(@"@custom-variant\s+[^;]+;", RegexOptions.IgnoreCase)]
    private static partial Regex CustomVariantDirectiveRegex();
}
