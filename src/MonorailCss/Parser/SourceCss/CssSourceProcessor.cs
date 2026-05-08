using System.Collections.Immutable;

namespace MonorailCss.Parser.SourceCss;

/// <summary>
/// Outcome of feeding a Tailwind v4-style CSS source through <see cref="CssSourceProcessor"/>.
/// </summary>
/// <param name="Settings">A <see cref="CssFrameworkSettings"/> populated from the source. Pass it
/// straight to <c>new CssFramework(result.Settings)</c>.</param>
/// <param name="RawCss">The pass-through CSS that the framework didn't consume — font-faces,
/// keyframes, plain selectors, layer base rules, etc. Concatenate this with the framework's
/// generated output when emitting the final stylesheet.</param>
/// <param name="ImportedFiles">Absolute paths of every file pulled into the import graph
/// (in the order they were processed). Empty when the source was processed in-memory without a
/// base path. Hot-reload watchers should monitor this list.</param>
/// <param name="SourceConfiguration">The aggregated <c>@import</c>/<c>@source</c>/<c>@custom-variant</c>
/// directives. <see cref="CssFrameworkSettings.CustomVariants"/> on <see cref="Settings"/> already
/// reflects <c>@custom-variant</c>; the rest of this object is informational for consumers that
/// want to drive their own scanner from <c>@source</c> hints.</param>
public record CssSourceResult(
    CssFrameworkSettings Settings,
    string RawCss,
    ImmutableList<string> ImportedFiles,
    SourceConfiguration SourceConfiguration);

/// <summary>
/// Top-level entry point for "given Tailwind-shaped CSS, give me a configured framework."
/// Wraps <see cref="CssImportProcessor"/> and translates its result into the framework's
/// <see cref="CssFrameworkSettings"/>:
/// <list type="bullet">
///   <item><description><c>@theme</c> + <c>@theme static</c> variables → <see cref="Theme.Theme.Add"/>.</description></item>
///   <item><description><c>@theme inline</c> + <c>@theme static inline</c> variables → <see cref="Theme.Theme.AddInline"/>
///     (so <c>var()</c> references known to the running theme are resolved at theme-define time).</description></item>
///   <item><description>Component selectors carrying <c>@apply</c> → <see cref="CssFrameworkSettings.Applies"/>.</description></item>
///   <item><description><c>@utility</c> blocks → <see cref="CssFrameworkSettings.CustomUtilities"/>.</description></item>
///   <item><description><c>@custom-variant</c> directives → <see cref="CssFrameworkSettings.CustomVariants"/>.</description></item>
/// </list>
/// </summary>
public class CssSourceProcessor
{
    private readonly Action<string>? _log;

    /// <summary>
    /// Initializes a new instance of the <see cref="CssSourceProcessor"/> class.
    /// </summary>
    /// <param name="log">Optional progress / warning callback.</param>
    public CssSourceProcessor(Action<string>? log = null)
    {
        _log = log;
    }

    /// <summary>
    /// Processes a CSS file and every file it transitively imports.
    /// </summary>
    /// <param name="cssFilePath">Path to the entry CSS file.</param>
    /// <param name="baseSettings">Optional starting settings (defaults to a fresh
    /// <see cref="CssFrameworkSettings"/> with <see cref="Theme.Theme.CreateWithDefaults"/>
    /// when not supplied — the typical Tailwind 4 starting point).</param>
    /// <returns>The processed result.</returns>
    public CssSourceResult ProcessFile(string cssFilePath, CssFrameworkSettings? baseSettings = null)
    {
        var importer = new CssImportProcessor(_log);
        var imported = importer.ProcessImports(cssFilePath);
        return BuildResult(imported, baseSettings);
    }

    /// <summary>
    /// Processes an in-memory CSS string. <c>@import</c>s reference files relative to
    /// <paramref name="basePath"/> (when provided); without a base path, imports are recorded
    /// in the result's <see cref="CssSourceResult.SourceConfiguration"/> but not loaded.
    /// </summary>
    /// <param name="cssSource">The CSS source.</param>
    /// <param name="basePath">Directory used to resolve relative <c>@import</c> paths.</param>
    /// <param name="baseSettings">Optional starting settings.</param>
    /// <returns>The processed result.</returns>
    public CssSourceResult ProcessSource(string cssSource, string? basePath = null, CssFrameworkSettings? baseSettings = null)
    {
        var importer = new CssImportProcessor(_log);
        var imported = importer.ProcessSource(cssSource, basePath);
        return BuildResult(imported, baseSettings);
    }

    private static CssSourceResult BuildResult(CssImportResult imported, CssFrameworkSettings? baseSettings)
    {
        var settings = baseSettings ?? new CssFrameworkSettings { Theme = Theme.Theme.CreateWithDefaults() };

        var theme = settings.Theme;
        foreach (var (k, v) in imported.ThemeVariables)
        {
            theme = theme.Add(k, v);
        }

        foreach (var (k, v) in imported.StaticThemeVariables)
        {
            // "static" controls whether the variable is always emitted; the value semantics
            // are the same as a regular @theme entry. The framework's ColorEmissionMode is
            // the closest equivalent control today; treat as a regular Add for now.
            theme = theme.Add(k, v);
        }

        foreach (var (k, v) in imported.InlineThemeVariables)
        {
            theme = theme.AddInline(k, v);
        }

        foreach (var (k, v) in imported.StaticInlineThemeVariables)
        {
            theme = theme.AddInline(k, v);
        }

        var applies = settings.Applies;
        foreach (var (k, v) in imported.ComponentRules)
        {
            applies = applies.SetItem(k, v);
        }

        var customUtilities = settings.CustomUtilities.AddRange(imported.UtilityDefinitions);
        var customVariants = settings.CustomVariants.AddRange(imported.SourceConfiguration.CustomVariants);

        var keyframes = settings.Keyframes;
        foreach (var (name, body) in imported.Keyframes)
        {
            keyframes = keyframes.SetItem(name, body);
        }

        var newSettings = settings with
        {
            Theme = theme,
            Applies = applies,
            CustomUtilities = customUtilities,
            CustomVariants = customVariants,
            Keyframes = keyframes,
        };

        var rawCss = imported.RawCssRules.Count == 0
            ? string.Empty
            : string.Join(Environment.NewLine + Environment.NewLine, imported.RawCssRules.Select(r => r.Content));

        return new CssSourceResult(newSettings, rawCss, imported.ImportedFiles, imported.SourceConfiguration);
    }
}
