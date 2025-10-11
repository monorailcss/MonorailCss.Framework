using System.Collections.Immutable;
using System.IO.Abstractions;
using Microsoft.Build.Utilities;

namespace MonorailCss.Build.Tasks.Parsing;

/// <summary>
/// Represents the result of processing CSS imports.
/// </summary>
internal record CssImportResult(
    ImmutableDictionary<string, string> ThemeVariables,
    ImmutableDictionary<string, string> InlineThemeVariables,
    ImmutableDictionary<string, string> StaticThemeVariables,
    ImmutableDictionary<string, string> StaticInlineThemeVariables,
    ImmutableDictionary<string, string> ComponentRules,
    ImmutableList<ParsedUtilityDefinition> UtilityDefinitions,
    SourceConfiguration SourceConfiguration,
    ImmutableList<RawCssRule> RawCssRules,
    ImmutableList<string> ImportedFiles);

/// <summary>
/// Represents a raw CSS rule (selector + declarations).
/// </summary>
internal record RawCssRule(string Selector, string Content, string SourceFile);

/// <summary>
/// Processes CSS @import directives recursively, resolving and merging imported files.
/// </summary>
internal partial class CssImportProcessor
{
    private readonly IFileSystem _fileSystem;
    private readonly CssThemeParser _themeParser;
    private readonly HashSet<string> _processedFiles;
    private readonly TaskLoggingHelper? _log;

    public CssImportProcessor(IFileSystem fileSystem, TaskLoggingHelper? log = null)
    {
        _fileSystem = fileSystem;
        _themeParser = new CssThemeParser();
        _processedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _log = log;
    }

    /// <summary>
    /// Processes a CSS file and all its imports recursively.
    /// </summary>
    /// <param name="cssFilePath">The path to the main CSS file.</param>
    /// <returns>The merged result of all processed files.</returns>
    public CssImportResult ProcessImports(string cssFilePath)
    {
        _processedFiles.Clear();

        var themeVariables = ImmutableDictionary.CreateBuilder<string, string>();
        var inlineThemeVariables = ImmutableDictionary.CreateBuilder<string, string>();
        var staticThemeVariables = ImmutableDictionary.CreateBuilder<string, string>();
        var staticInlineThemeVariables = ImmutableDictionary.CreateBuilder<string, string>();
        var componentRules = ImmutableDictionary.CreateBuilder<string, string>();
        var utilityDefinitions = ImmutableList.CreateBuilder<ParsedUtilityDefinition>();
        var rawCssRules = ImmutableList.CreateBuilder<RawCssRule>();
        var importedFiles = ImmutableList.CreateBuilder<string>();
        var sourceConfiguration = new SourceConfiguration();

        ProcessFileRecursive(
            cssFilePath,
            themeVariables,
            inlineThemeVariables,
            staticThemeVariables,
            staticInlineThemeVariables,
            componentRules,
            utilityDefinitions,
            rawCssRules,
            importedFiles,
            ref sourceConfiguration);

        return new CssImportResult(
            themeVariables.ToImmutable(),
            inlineThemeVariables.ToImmutable(),
            staticThemeVariables.ToImmutable(),
            staticInlineThemeVariables.ToImmutable(),
            componentRules.ToImmutable(),
            utilityDefinitions.ToImmutable(),
            sourceConfiguration,
            rawCssRules.ToImmutable(),
            importedFiles.ToImmutable());
    }

    private void ProcessFileRecursive(
        string cssFilePath,
        ImmutableDictionary<string, string>.Builder themeVariables,
        ImmutableDictionary<string, string>.Builder inlineThemeVariables,
        ImmutableDictionary<string, string>.Builder staticThemeVariables,
        ImmutableDictionary<string, string>.Builder staticInlineThemeVariables,
        ImmutableDictionary<string, string>.Builder componentRules,
        ImmutableList<ParsedUtilityDefinition>.Builder utilityDefinitions,
        ImmutableList<RawCssRule>.Builder rawCssRules,
        ImmutableList<string>.Builder importedFiles,
        ref SourceConfiguration sourceConfiguration)
    {
        // Normalize path
        var normalizedPath = _fileSystem.Path.GetFullPath(cssFilePath);

        // Check for circular dependencies
        if (_processedFiles.Contains(normalizedPath))
        {
            _log?.LogMessage(Microsoft.Build.Framework.MessageImportance.Low,
                $"Skipping already processed file: {normalizedPath}");
            return;
        }

        if (!_fileSystem.File.Exists(normalizedPath))
        {
            _log?.LogWarning($"CSS import not found: {normalizedPath}");
            return;
        }

        _processedFiles.Add(normalizedPath);
        importedFiles.Add(normalizedPath);

        _log?.LogMessage(Microsoft.Build.Framework.MessageImportance.Low,
            $"Processing CSS import: {normalizedPath}");

        // Read and parse the CSS file
        var cssContent = _fileSystem.File.ReadAllText(normalizedPath);
        var parseResult = _themeParser.Parse(cssContent);

        // Merge theme variables (later declarations override earlier ones)
        foreach (var kvp in parseResult.ThemeVariables)
        {
            themeVariables[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in parseResult.InlineThemeVariables)
        {
            inlineThemeVariables[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in parseResult.StaticThemeVariables)
        {
            staticThemeVariables[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in parseResult.StaticInlineThemeVariables)
        {
            staticInlineThemeVariables[kvp.Key] = kvp.Value;
        }

        // Merge component rules
        foreach (var kvp in parseResult.ComponentRules)
        {
            componentRules[kvp.Key] = kvp.Value;
        }

        // Add utility definitions
        utilityDefinitions.AddRange(parseResult.UtilityDefinitions);

        // Merge source configuration (first file wins for base path)
        if (sourceConfiguration.BasePath == null && parseResult.SourceConfiguration.BasePath != null)
        {
            sourceConfiguration = parseResult.SourceConfiguration;
        }
        else
        {
            // Merge include/exclude sources
            var mergedInclude = sourceConfiguration.IncludeSources.AddRange(parseResult.SourceConfiguration.IncludeSources);
            var mergedExclude = sourceConfiguration.ExcludeSources.AddRange(parseResult.SourceConfiguration.ExcludeSources);
            var mergedInline = sourceConfiguration.InlineSources.AddRange(parseResult.SourceConfiguration.InlineSources);
            var mergedVariants = sourceConfiguration.CustomVariants.AddRange(parseResult.SourceConfiguration.CustomVariants);
            var mergedImports = sourceConfiguration.Imports.AddRange(parseResult.SourceConfiguration.Imports);

            sourceConfiguration = sourceConfiguration with
            {
                IncludeSources = mergedInclude,
                ExcludeSources = mergedExclude,
                InlineSources = mergedInline,
                CustomVariants = mergedVariants,
                Imports = mergedImports,
                DisableAutoDetection = sourceConfiguration.DisableAutoDetection || parseResult.SourceConfiguration.DisableAutoDetection
            };
        }

        // Extract raw CSS rules (selectors with declarations, @font-face, @keyframes, etc.)
        ExtractRawCssRules(cssContent, normalizedPath, rawCssRules);

        // Process @import directives recursively
        var currentDirectory = _fileSystem.Path.GetDirectoryName(normalizedPath) ?? string.Empty;
        foreach (var import in parseResult.SourceConfiguration.Imports)
        {
            // Skip special imports like "tailwindcss"
            if (import.Path.Equals("tailwindcss", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Resolve import path relative to current file
            var importPath = ResolveImportPath(import.Path, currentDirectory);

            _log?.LogMessage(Microsoft.Build.Framework.MessageImportance.Low,
                $"Resolving @import \"{import.Path}\" -> {importPath}");

            // Recursively process the imported file
            ProcessFileRecursive(
                importPath,
                themeVariables,
                inlineThemeVariables,
                staticThemeVariables,
                staticInlineThemeVariables,
                componentRules,
                utilityDefinitions,
                rawCssRules,
                importedFiles,
                ref sourceConfiguration);
        }
    }

    private string ResolveImportPath(string importPath, string currentDirectory)
    {
        // Remove quotes if present
        var cleanPath = importPath.Trim('"', '\'');

        // Try to resolve as relative path
        if (!_fileSystem.Path.IsPathRooted(cleanPath))
        {
            cleanPath = _fileSystem.Path.Combine(currentDirectory, cleanPath);
        }

        // Try with .css extension if not present
        if (!_fileSystem.File.Exists(cleanPath) && !cleanPath.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
        {
            var withExtension = cleanPath + ".css";
            if (_fileSystem.File.Exists(withExtension))
            {
                return _fileSystem.Path.GetFullPath(withExtension);
            }
        }

        return _fileSystem.Path.GetFullPath(cleanPath);
    }

    private void ExtractRawCssRules(string cssContent, string sourceFile, ImmutableList<RawCssRule>.Builder rules)
    {
        // Process @layer blocks with @apply directives before removing other directives
        var content = ProcessLayerBlocks(cssContent);

        // Remove @theme blocks and @utility blocks as they're handled separately
        content = RemoveThemeBlocks(content);
        content = RemoveUtilityBlocks(content);
        content = RemoveSourceDirectives(content);
        content = RemoveImportDirectives(content);
        content = RemoveCustomVariantDirectives(content);

        // Now extract remaining CSS rules (selectors, @font-face, @keyframes, @layer, etc.)
        // We'll keep everything that remains as raw CSS to be included in the output

        var trimmedContent = content.Trim();
        if (!string.IsNullOrWhiteSpace(trimmedContent))
        {
            // Store the entire remaining content as a single rule
            // The selector will be empty to indicate this is raw CSS to be included as-is
            rules.Add(new RawCssRule(string.Empty, trimmedContent, sourceFile));

            _log?.LogMessage(Microsoft.Build.Framework.MessageImportance.Low,
                $"Extracted {trimmedContent.Length} characters of raw CSS from {_fileSystem.Path.GetFileName(sourceFile)}");
        }
    }

    private string RemoveThemeBlocks(string css)
    {
        // Remove @theme blocks (with optional static/inline modifiers)
        var result = css;
        var i = 0;
        var output = new System.Text.StringBuilder();

        while (i < result.Length)
        {
            var themeIndex = result.IndexOf("@theme", i, StringComparison.OrdinalIgnoreCase);
            if (themeIndex == -1)
            {
                // No more @theme blocks, append the rest
                output.Append(result.AsSpan(i));
                break;
            }

            // Append everything before @theme
            output.Append(result.AsSpan(i, themeIndex - i));

            // Find the opening brace
            var openBrace = result.IndexOf('{', themeIndex);
            if (openBrace == -1)
            {
                break;
            }

            // Find matching closing brace
            var braceDepth = 1;
            var j = openBrace + 1;
            while (j < result.Length && braceDepth > 0)
            {
                if (result[j] == '{') braceDepth++;
                else if (result[j] == '}') braceDepth--;
                j++;
            }

            // Skip past the @theme block
            i = j;
        }

        return output.ToString();
    }

    private string RemoveUtilityBlocks(string css)
    {
        // Remove @utility blocks
        var result = css;
        var i = 0;
        var output = new System.Text.StringBuilder();

        while (i < result.Length)
        {
            var utilityIndex = result.IndexOf("@utility", i, StringComparison.OrdinalIgnoreCase);
            if (utilityIndex == -1)
            {
                output.Append(result.AsSpan(i));
                break;
            }

            output.Append(result.AsSpan(i, utilityIndex - i));

            var openBrace = result.IndexOf('{', utilityIndex);
            if (openBrace == -1)
            {
                break;
            }

            var braceDepth = 1;
            var j = openBrace + 1;
            while (j < result.Length && braceDepth > 0)
            {
                if (result[j] == '{') braceDepth++;
                else if (result[j] == '}') braceDepth--;
                j++;
            }

            i = j;
        }

        return output.ToString();
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"@source\s+[^;]+;", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
    private static partial System.Text.RegularExpressions.Regex SourceDirectiveRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@"@import\s+[^;]+;", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
    private static partial System.Text.RegularExpressions.Regex ImportDirectiveRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@"@custom-variant\s+[^;]+;", System.Text.RegularExpressions.RegexOptions.IgnoreCase)]
    private static partial System.Text.RegularExpressions.Regex CustomVariantDirectiveRegex();

    private string RemoveSourceDirectives(string css)
    {
        // Remove @source directives (single line)
        return SourceDirectiveRegex().Replace(css, string.Empty);
    }

    private string RemoveImportDirectives(string css)
    {
        // Remove @import directives (single line)
        return ImportDirectiveRegex().Replace(css, string.Empty);
    }

    private string RemoveCustomVariantDirectives(string css)
    {
        // Remove @custom-variant directives (single line)
        return CustomVariantDirectiveRegex().Replace(css, string.Empty);
    }

    /// <summary>
    /// Processes @layer blocks and expands @apply directives within them.
    /// This is a simplified implementation that preserves @layer structure while expanding @apply.
    /// </summary>
    private string ProcessLayerBlocks(string css)
    {
        // For now, we'll preserve @layer blocks as-is
        // A full implementation would need to:
        // 1. Parse @layer blocks
        // 2. Find @apply directives within them
        // 3. Expand the @apply directives using the CssFramework
        // 4. Replace the @apply with expanded CSS

        // For v1, we just log that we found them and pass through
        if (css.Contains("@layer", StringComparison.OrdinalIgnoreCase))
        {
            _log?.LogMessage(Microsoft.Build.Framework.MessageImportance.Low,
                "Found @layer directive - preserving as-is (full @layer processing not yet implemented)");
        }

        return css;
    }
}
