using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using MonorailCss.Parser.SourceCss;

namespace MonorailCss.Discovery;

/// <summary>
/// Inputs to a single <see cref="MonorailCssGenerator.Generate"/> call. Both the runtime
/// discovery service and the build-time MSBuild task project their own configuration onto
/// this shape; the generator handles the actual scanning + composition pipeline.
/// </summary>
public sealed record MonorailCssGenerationRequest
{
    /// <summary>Gets the inline CSS source — <c>@theme</c>, <c>@apply</c>, <c>@utility</c>,
    /// <c>@custom-variant</c>, plus pass-through CSS. When set, the generator runs it
    /// through <see cref="CssSourceProcessor"/> to derive the framework configuration.
    /// Combined with <see cref="SourceCssPath"/> when both are set (the path's directory
    /// becomes the base for <c>@import</c> resolution; the inline content layers on top).</summary>
    public string? SourceCss { get; init; }

    /// <summary>Gets the path to a CSS file that drives the framework configuration. When
    /// set, the generator follows <c>@import</c> directives recursively from the file's
    /// directory. The result's <see cref="MonorailCssGenerationResult.ImportedCssFiles"/>
    /// lists every file pulled in — runtime callers watch these for hot-reload.</summary>
    public string? SourceCssPath { get; init; }

    /// <summary>Gets the starting framework when neither <see cref="SourceCss"/> nor
    /// <see cref="SourceCssPath"/> is set. Defaults to <c>new CssFramework()</c>. Use this
    /// when you've configured a framework imperatively (custom theme, registered utilities)
    /// rather than via CSS source directives.</summary>
    public CssFramework? BaseFramework { get; init; }

    /// <summary>Gets the loaded assemblies to scan via in-memory metadata
    /// (<c>Assembly.TryGetRawMetadata</c>). Runtime supplies
    /// <c>AppDomain.CurrentDomain.GetAssemblies()</c> here; build task leaves empty.</summary>
    public IReadOnlyList<Assembly> Assemblies { get; init; } = [];

    /// <summary>Gets the on-disk DLL paths to scan via <see cref="PEReader"/>. Build task
    /// supplies MSBuild's <c>@(ReferencePath)</c> here; runtime leaves empty.</summary>
    public IReadOnlyList<string> AssemblyFiles { get; init; } = [];

    /// <summary>Gets the source files (<c>.razor</c>, <c>.cshtml</c>, <c>.cs</c>, etc.) to
    /// scan via <see cref="SourceFileScanner"/>. Both transports use this — runtime for
    /// hot-reload-time source files, build task for explicit + auto-detected content.</summary>
    public IReadOnlyList<string> SourceFiles { get; init; } = [];

    /// <summary>Gets the class names to always include in the generated CSS, regardless of
    /// whether they were discovered. Use for runtime-constructed strings the static
    /// scanners can't reconstruct (e.g. <c>$"bg-{color}-500"</c>) or for inline safelist
    /// directives (<c>@source inline("…")</c>).</summary>
    public IReadOnlyList<string> ExtraSafelist { get; init; } = [];

    /// <summary>Gets the assembly names (no <c>.dll</c>, no version) to skip during
    /// scanning. Matched against <c>Assembly.GetName().Name</c> for loaded assemblies and
    /// <c>Path.GetFileNameWithoutExtension</c> for on-disk DLLs. Useful for the MonorailCss
    /// framework itself (it ships <c>"bg-{color}-500"</c>-shaped templates), icon packs, or
    /// anything whose IL-embedded strings would inflate the candidate set without
    /// contributing real utilities.</summary>
    public IReadOnlyCollection<string> ExcludeAssemblies { get; init; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Output of <see cref="MonorailCssGenerator.Generate"/>: the composed CSS plus diagnostics
/// the runtime middleware and build task both need (ETag for cache validation, the live
/// framework for downstream introspection, the imported file set for hot-reload watchers).
/// </summary>
/// <param name="Css">Final CSS — raw passthrough from the source CSS pipeline followed by
/// the framework's generated utilities.</param>
/// <param name="ETag">RFC 7232 entity tag (quoted hex prefix of SHA-256(Css)). Stable across
/// runs that produce identical output.</param>
/// <param name="Classes">Every candidate that survived validation, sorted. Includes the
/// safelist.</param>
/// <param name="ImportedCssFiles">Absolute paths of every CSS file the source-CSS pipeline
/// pulled in (entry + transitive imports). Empty when neither <c>SourceCss</c> nor
/// <c>SourceCssPath</c> was set.</param>
/// <param name="Framework">The framework instance the generator built for this run. Useful
/// when the caller wants to introspect resolved theme variables, registered custom
/// utilities, etc.</param>
/// <param name="SourceConfiguration">The aggregated <c>@import</c> / <c>@source</c> /
/// <c>@source not</c> / <c>@source inline()</c> / <c>@custom-variant</c> directives parsed
/// from the source CSS. Build-time consumers use this to drive their own glob expansion;
/// runtime consumers can ignore it.</param>
public sealed record MonorailCssGenerationResult(
    string Css,
    string ETag,
    ImmutableSortedSet<string> Classes,
    ImmutableList<string> ImportedCssFiles,
    CssFramework Framework,
    SourceConfiguration SourceConfiguration);

/// <summary>
/// The single discovery + generation pipeline both transports call into.
/// <see cref="MonorailCss.Discovery"/> wraps it with an <c>IHostedService</c>, file watchers,
/// and a middleware to serve the result over HTTP. <c>MonorailCss.Build.Tasks</c> wraps it
/// with an MSBuild task that translates <c>@(ReferencePath)</c> + <c>@source</c> directives
/// into a request and writes the result to disk. Behavior parity between the two is
/// guaranteed because they call the same code.
/// </summary>
/// <remarks>
/// <para>
/// Stateful by design: the generator caches per-MVID assembly scan results so a runtime
/// regeneration triggered by hot-reload short-circuits unchanged assemblies.
/// Build-time use is one-shot, where the cache is harmless dead weight.
/// </para>
/// <para>
/// When the request resolves to a different framework than the previous call (typically
/// because the source CSS changed), the per-assembly cache is cleared automatically
/// because validation answers may have shifted under the new framework.
/// </para>
/// </remarks>
public sealed class MonorailCssGenerator
{
    private readonly object _lock = new();
    private CssFramework? _lastFramework;
    private ValidationCache? _validationCache;
    private AssemblyClassScanner? _assemblyScanner;
    private SourceFileScanner? _sourceFileScanner;
    private HashSet<string>? _lastClasses;
    private string? _lastRawCss;
    private MonorailCssGenerationResult? _lastResult;
    private ResolveCache? _resolveCache;

    /// <summary>
    /// Runs the discovery pipeline against <paramref name="request"/> and returns the
    /// composed CSS plus diagnostics. Thread-safe; concurrent calls serialize on an
    /// internal lock so the per-MVID cache stays consistent.
    /// </summary>
    /// <param name="request">Inputs to scan and configuration to drive the framework.</param>
    /// <returns>The composed result; never null.</returns>
    public MonorailCssGenerationResult Generate(MonorailCssGenerationRequest request)
    {
        lock (_lock)
        {
            // Step 1: resolve framework (process source CSS if any, else use the supplied base).
            var (framework, rawCss, importedFiles, sourceConfiguration) = ResolveFrameworkCached(request);

            // Step 2: rebuild caches when the framework changed (validation answers may differ).
            if (!ReferenceEquals(framework, _lastFramework))
            {
                _validationCache = new ValidationCache(framework);
                _assemblyScanner = new AssemblyClassScanner(_validationCache);
                _sourceFileScanner = new SourceFileScanner(_validationCache);
                _lastFramework = framework;
                _lastResult = null;
                _lastClasses = null;
                _lastRawCss = null;
            }

            var validationCache = _validationCache!;
            var assemblyScanner = _assemblyScanner!;
            var sourceFileScanner = _sourceFileScanner!;

            // Step 3: scan all sources into a unified candidate set.
            var classes = new HashSet<string>(StringComparer.Ordinal);
            ScanLoadedAssemblies(request, assemblyScanner, classes);
            ScanAssemblyFiles(request, validationCache, classes);
            ScanSourceFiles(request, sourceFileScanner, classes);

            // Step 4: merge safelist (always included, no validation gate).
            foreach (var safe in request.ExtraSafelist)
            {
                if (!string.IsNullOrWhiteSpace(safe))
                {
                    classes.Add(safe);
                }
            }

            // Short-circuit: same framework + same raw CSS + same candidate set → reuse last result.
            // framework.Process() dominates the call cost; HashSet.SetEquals is O(n) and cheap.
            if (_lastResult is not null
                && _lastClasses is not null
                && string.Equals(rawCss, _lastRawCss, StringComparison.Ordinal)
                && classes.SetEquals(_lastClasses))
            {
                return _lastResult;
            }

            var sortedClasses = classes.ToImmutableSortedSet(StringComparer.Ordinal);

            // Step 5: compose final CSS.
            var generatedCss = framework.Process(sortedClasses);
            var finalCss = string.IsNullOrWhiteSpace(rawCss)
                ? generatedCss
                : rawCss + Environment.NewLine + Environment.NewLine + generatedCss;

            var result = new MonorailCssGenerationResult(
                Css: finalCss,
                ETag: ComputeETag(finalCss),
                Classes: sortedClasses,
                ImportedCssFiles: importedFiles,
                Framework: framework,
                SourceConfiguration: sourceConfiguration);

            _lastClasses = new HashSet<string>(classes, StringComparer.Ordinal);
            _lastRawCss = rawCss;
            _lastResult = result;
            return result;
        }
    }

    private (CssFramework Framework, string RawCss, ImmutableList<string> ImportedFiles, SourceConfiguration SourceConfiguration) ResolveFrameworkCached(MonorailCssGenerationRequest request)
    {
        var hasSourceCss = !string.IsNullOrWhiteSpace(request.SourceCss);
        var hasSourceCssPath = !string.IsNullOrWhiteSpace(request.SourceCssPath);

        // No source CSS: framework is BaseFramework (or a default). Cache by reference identity
        // so callers that reuse the same BaseFramework across calls get a stable result.
        if (!hasSourceCss && !hasSourceCssPath)
        {
            if (_resolveCache is { SourceCss: null, SourceCssPath: null } cached
                && ReferenceEquals(cached.BaseFramework, request.BaseFramework))
            {
                return (cached.Framework, cached.RawCss, cached.ImportedFiles, cached.SourceConfiguration);
            }

            var fw = request.BaseFramework ?? new CssFramework();
            _resolveCache = new ResolveCache(
                SourceCss: null,
                SourceCssPath: null,
                BaseFramework: request.BaseFramework,
                Framework: fw,
                RawCss: string.Empty,
                ImportedFiles: ImmutableList<string>.Empty,
                SourceConfiguration: new SourceConfiguration(),
                FileTimes: ImmutableDictionary<string, DateTime>.Empty);
            return (fw, string.Empty, ImmutableList<string>.Empty, new SourceConfiguration());
        }

        // Source CSS present: cache by (SourceCss, SourceCssPath, file mtimes). BaseFramework is
        // intentionally ignored from the key — the discovery service rewrites _options.Framework
        // to result.Framework after every call, so its reference changes every call even when
        // the CSS hasn't. The CSS content fully determines what the framework should look like.
        if (_resolveCache is { } c
            && string.Equals(c.SourceCss, request.SourceCss, StringComparison.Ordinal)
            && string.Equals(c.SourceCssPath, request.SourceCssPath, StringComparison.Ordinal)
            && FileTimesUnchanged(c.FileTimes))
        {
            return (c.Framework, c.RawCss, c.ImportedFiles, c.SourceConfiguration);
        }

        var processor = new CssSourceProcessor();
        var baseSettings = request.BaseFramework?.Settings;

        CssSourceResult result;
        if (hasSourceCssPath)
        {
            result = processor.ProcessFile(request.SourceCssPath!, baseSettings);

            if (hasSourceCss)
            {
                // Inline content layers on top of file-derived settings; the file's directory
                // becomes the base for any @imports the inline content carries.
                var basePath = Path.GetDirectoryName(request.SourceCssPath);
                result = processor.ProcessSource(request.SourceCss!, basePath, result.Settings);
            }
        }
        else
        {
            result = processor.ProcessSource(request.SourceCss!, basePath: null, baseSettings);
        }

        var resolvedFramework = new CssFramework(result.Settings);
        _resolveCache = new ResolveCache(
            SourceCss: request.SourceCss,
            SourceCssPath: request.SourceCssPath,
            BaseFramework: request.BaseFramework,
            Framework: resolvedFramework,
            RawCss: result.RawCss,
            ImportedFiles: result.ImportedFiles,
            SourceConfiguration: result.SourceConfiguration,
            FileTimes: SnapshotFileTimes(result.ImportedFiles));
        return (resolvedFramework, result.RawCss, result.ImportedFiles, result.SourceConfiguration);
    }

    private static ImmutableDictionary<string, DateTime> SnapshotFileTimes(ImmutableList<string> files)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in files)
        {
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            try
            {
                builder[path] = File.GetLastWriteTimeUtc(path);
            }
            catch
            {
                // If we can't stat the file now, record a sentinel so any later read can't match.
                builder[path] = DateTime.MinValue;
            }
        }

        return builder.ToImmutable();
    }

    private static bool FileTimesUnchanged(ImmutableDictionary<string, DateTime> snapshot)
    {
        foreach (var (path, stamp) in snapshot)
        {
            DateTime current;
            try
            {
                current = File.GetLastWriteTimeUtc(path);
            }
            catch
            {
                return false;
            }

            if (current != stamp)
            {
                return false;
            }
        }

        return true;
    }

    private sealed record ResolveCache(
        string? SourceCss,
        string? SourceCssPath,
        CssFramework? BaseFramework,
        CssFramework Framework,
        string RawCss,
        ImmutableList<string> ImportedFiles,
        SourceConfiguration SourceConfiguration,
        ImmutableDictionary<string, DateTime> FileTimes);

    private static void ScanLoadedAssemblies(
        MonorailCssGenerationRequest request,
        AssemblyClassScanner scanner,
        ICollection<string> output)
    {
        foreach (var assembly in request.Assemblies)
        {
            if (assembly.IsDynamic)
            {
                continue;
            }

            var name = assembly.GetName().Name;
            if (string.IsNullOrEmpty(name)
                || IlMetadataScanner.IsKnownFrameworkAssembly(name)
                || request.ExcludeAssemblies.Contains(name))
            {
                continue;
            }

            scanner.Scan(assembly, output);
        }
    }

    private static void ScanAssemblyFiles(
        MonorailCssGenerationRequest request,
        ValidationCache validationCache,
        ICollection<string> output)
    {
        foreach (var path in request.AssemblyFiles)
        {
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            var name = Path.GetFileNameWithoutExtension(path);
            if (IlMetadataScanner.IsKnownFrameworkAssembly(name)
                || request.ExcludeAssemblies.Contains(name))
            {
                continue;
            }

            try
            {
                using var stream = File.OpenRead(path);
                using var peReader = new PEReader(stream);
                if (!peReader.HasMetadata)
                {
                    continue;
                }

                var reader = peReader.GetMetadataReader();
                if (!reader.IsAssembly || IlMetadataScanner.HasReferenceAssemblyAttribute(reader))
                {
                    continue;
                }

                IlMetadataScanner.ScanUserStringHeap(reader, validationCache, output);
            }
            catch
            {
                // Best-effort: a malformed DLL or a transient read error shouldn't fail the
                // whole generation. The candidate set is still useful from the rest.
            }
        }
    }

    private static void ScanSourceFiles(
        MonorailCssGenerationRequest request,
        SourceFileScanner scanner,
        ICollection<string> output)
    {
        foreach (var path in request.SourceFiles)
        {
            if (!string.IsNullOrEmpty(path))
            {
                scanner.ScanFile(path, output);
            }
        }
    }

    private static string ComputeETag(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        var hex = Convert.ToHexString(hash, 0, 12);
        return "\"" + hex + "\"";
    }
}
