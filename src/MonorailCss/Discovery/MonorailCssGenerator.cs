using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
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

    /// <summary>Gets the on-disk DLL paths to scan via <c>PEReader</c>. Build task
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

    /// <summary>Gets a value indicating whether the generator may reuse the source-file token
    /// contribution from the previous call instead of walking and re-scanning
    /// <see cref="SourceFiles"/>. Set this when the caller knows source files cannot have
    /// changed since the previous <see cref="MonorailCssGenerator.Generate"/> call (e.g.
    /// a late-load assembly event, a hot-reload metadata-update event). When true and the
    /// generator has a previous result on hand, the directory walk and per-file scans are
    /// skipped entirely. The first call with this flag set still does a full scan, since
    /// there is nothing cached yet.</summary>
    public bool SkipSourceFileScan { get; init; }

    /// <summary>Gets the source files that may have changed since the last call. When non-null
    /// and a previous source-file token contribution is cached, the generator scans only
    /// these files (force-bypassing the per-file mtime cache for them) and folds the
    /// resulting tokens into the previous contribution. When null, behaviour is unchanged:
    /// the full <see cref="SourceFiles"/> list is walked and each file's mtime decides
    /// whether it gets rescanned. An empty (non-null) collection is a valid signal meaning
    /// "nothing changed". Note: tokens contributed by a previous version of a changed file
    /// are not removed — a class no longer used in the source lingers in the output until
    /// a full rescan (startup, css-watcher) reseeds the cache. This matches Tailwind's
    /// trade-off and avoids needing a token-to-file reverse index.</summary>
    public IReadOnlyCollection<string>? ChangedSourceFiles { get; init; }
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
    private HashSet<string>? _lastSourceFileTokens;

    // Pending seeds applied to the scanners on the next Generate() call, once the framework
    // (and the scanners it owns) have been resolved. SeedCache may be called before any
    // Generate() — at which point no scanners exist yet — so we hold the seed here and apply
    // it after framework resolution.
    private IReadOnlyList<SourceFileCacheEntry>? _pendingSourceSeed;
    private IReadOnlyList<AssemblyCacheEntry>? _pendingAssemblySeed;
    private IReadOnlyCollection<string>? _pendingSourceTokenUnion;

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
                _lastSourceFileTokens = null;
            }

            var assemblyScanner = _assemblyScanner!;
            var sourceFileScanner = _sourceFileScanner!;

            // Apply any pending persistent-cache seed now that scanners exist. Done here (not in
            // SeedCache) because the scanners get rebuilt whenever the framework changes — a
            // seed applied to a stale scanner would be lost.
            ApplyPendingSeed(sourceFileScanner, assemblyScanner);

            // Step 3: scan all sources into a unified candidate set. Source files get their own
            // bucket so we can replay it when SkipSourceFileScan is set on a later call.
            var classes = new HashSet<string>(StringComparer.Ordinal);
            ScanLoadedAssemblies(request, assemblyScanner, classes);
            ScanAssemblyFiles(request, assemblyScanner, classes);

            if (request.ChangedSourceFiles is { } changedFiles && _lastSourceFileTokens is not null)
            {
                // Tailwind-style incremental: only the listed files are re-tokenised; everything
                // else carries over from the previous call. We deliberately do NOT remove tokens
                // contributed by a prior version of a changed file — matches Tailwind's
                // documented trade-off and avoids needing a token-to-file reverse index. The
                // stale tokens are flushed by any startup / css-watcher rescan.
                foreach (var path in changedFiles)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        sourceFileScanner.ScanFileForceRescan(path, _lastSourceFileTokens);
                    }
                }

                classes.UnionWith(_lastSourceFileTokens);
            }
            else if (request.SkipSourceFileScan && _lastSourceFileTokens is not null)
            {
                classes.UnionWith(_lastSourceFileTokens);
            }
            else
            {
                var sourceFileTokens = new HashSet<string>(StringComparer.Ordinal);
                ScanSourceFiles(request, sourceFileScanner, sourceFileTokens);
                classes.UnionWith(sourceFileTokens);
                _lastSourceFileTokens = sourceFileTokens;
            }

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

    /// <summary>
    /// Imports a prior cache snapshot (typically read from
    /// <c>obj/MonorailCss/*.json</c> at the start of an MSBuild task invocation) so the
    /// following <see cref="Generate"/> call can skip re-scanning assemblies and source files
    /// that haven't changed since the snapshot was taken.
    /// <para>
    /// The seed is held until <see cref="Generate"/> resolves the framework and instantiates
    /// the scanners — the scanners get rebuilt whenever the framework changes, so applying
    /// the seed eagerly would risk it being thrown away. Callers usually pair this with
    /// <see cref="MonorailCssGenerationRequest.ChangedSourceFiles"/> so only files known to
    /// have changed since the seed are actually re-scanned.
    /// </para>
    /// </summary>
    /// <param name="seed">Per-source-file and per-MVID entries from a prior
    /// <see cref="SnapshotCache"/> call.</param>
    public void SeedCache(GenerationCacheSeed seed)
    {
        lock (_lock)
        {
            _pendingSourceSeed = seed.SourceFiles;
            _pendingAssemblySeed = seed.Assemblies;

            // Pre-compute the union of all seeded source-file tokens so the ChangedSourceFiles
            // branch in Generate() has something to fold the freshly-scanned changed files into.
            if (seed.SourceFiles is { Count: > 0 })
            {
                var union = new HashSet<string>(StringComparer.Ordinal);
                foreach (var entry in seed.SourceFiles)
                {
                    foreach (var token in entry.Tokens)
                    {
                        union.Add(token);
                    }
                }

                _pendingSourceTokenUnion = union;
            }
            else
            {
                _pendingSourceTokenUnion = null;
            }
        }
    }

    /// <summary>
    /// Snapshots the current cache state — per-MVID assembly tokens (joined to their path +
    /// mtime) and per-source-file tokens — for persistence to disk. Pair with
    /// <see cref="SeedCache"/> in the next process to restore the state.
    /// </summary>
    /// <param name="sourceFilePathFilter">When non-null, only source-file entries whose path
    /// is in this set are returned. Use to drop files no longer in the scan list from the
    /// persisted cache.</param>
    /// <returns>A snapshot suitable for round-tripping through <see cref="SeedCache"/>.</returns>
    public GenerationCacheSnapshot SnapshotCache(IReadOnlyCollection<string>? sourceFilePathFilter = null)
    {
        lock (_lock)
        {
            var sourceFiles = _sourceFileScanner?.SnapshotCache(sourceFilePathFilter)
                              ?? (IReadOnlyList<SourceFileCacheEntry>)Array.Empty<SourceFileCacheEntry>();
            var assemblies = _assemblyScanner?.SnapshotCache()
                             ?? (IReadOnlyList<AssemblyCacheEntry>)Array.Empty<AssemblyCacheEntry>();

            return new GenerationCacheSnapshot(sourceFiles, assemblies);
        }
    }

    private void ApplyPendingSeed(SourceFileScanner sourceFileScanner, AssemblyClassScanner assemblyScanner)
    {
        if (_pendingSourceSeed is { Count: > 0 } sourceSeed)
        {
            sourceFileScanner.SeedCache(sourceSeed);
            _pendingSourceSeed = null;
        }

        if (_pendingAssemblySeed is { Count: > 0 } assemblySeed)
        {
            assemblyScanner.SeedCache(assemblySeed);
            _pendingAssemblySeed = null;
        }

        // The token union seeded above becomes _lastSourceFileTokens so the ChangedSourceFiles
        // branch in Generate() (which unions changed-file output into _lastSourceFileTokens)
        // has the prior contribution to start from. Only set when not already populated by a
        // prior in-process call.
        if (_pendingSourceTokenUnion is { } union && _lastSourceFileTokens is null)
        {
            _lastSourceFileTokens = new HashSet<string>(union, StringComparer.Ordinal);
            _pendingSourceTokenUnion = null;
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

    // Below this item count, Parallel.ForEach's task-spawn + per-partition merge overhead
    // outweighs the gain, so the scan runs inline on the calling thread. Mirrors oxide's
    // choice to use a synchronous walk for the first (small) build.
    private const int ParallelScanThreshold = 8;

    /// <summary>
    /// Scans <paramref name="items"/> into <paramref name="output"/>, in parallel once the
    /// item count clears <see cref="ParallelScanThreshold"/>. Each worker accumulates into a
    /// thread-local set and merges into the shared collection once, under a lock, so the only
    /// contention is the merge — the per-item scan runs lock-free. Relies on the scanners
    /// being safe to call concurrently (their caches are <c>ConcurrentDictionary</c>
    /// and the parse path holds no mutable state).
    /// </summary>
    private static void ScanItemsParallel<T>(
        IReadOnlyList<T> items,
        Action<T, ICollection<string>> scanItem,
        ICollection<string> output)
    {
        if (items.Count == 0)
        {
            return;
        }

        if (items.Count < ParallelScanThreshold)
        {
            foreach (var item in items)
            {
                scanItem(item, output);
            }

            return;
        }

        Parallel.ForEach(
            items,
            () => new HashSet<string>(StringComparer.Ordinal),
            (item, _, local) =>
            {
                scanItem(item, local);
                return local;
            },
            local =>
            {
                lock (output)
                {
                    foreach (var token in local)
                    {
                        output.Add(token);
                    }
                }
            });
    }

    private static void ScanLoadedAssemblies(
        MonorailCssGenerationRequest request,
        AssemblyClassScanner scanner,
        ICollection<string> output)
    {
        var targets = new List<Assembly>(request.Assemblies.Count);
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

            targets.Add(assembly);
        }

        ScanItemsParallel(targets, (assembly, sink) => scanner.Scan(assembly, sink), output);
    }

    private static void ScanAssemblyFiles(
        MonorailCssGenerationRequest request,
        AssemblyClassScanner scanner,
        ICollection<string> output)
    {
        var targets = new List<string>(request.AssemblyFiles.Count);
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

            targets.Add(path);
        }

        ScanItemsParallel(targets, (path, sink) => scanner.ScanFile(path, sink), output);
    }

    private static void ScanSourceFiles(
        MonorailCssGenerationRequest request,
        SourceFileScanner scanner,
        ICollection<string> output)
    {
        var targets = new List<string>(request.SourceFiles.Count);
        foreach (var path in request.SourceFiles)
        {
            if (!string.IsNullOrEmpty(path))
            {
                targets.Add(path);
            }
        }

        ScanItemsParallel(targets, (path, sink) => scanner.ScanFile(path, sink), output);
    }

    private static string ComputeETag(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        var hex = Convert.ToHexString(hash, 0, 12);
        return "\"" + hex + "\"";
    }
}

/// <summary>
/// Cache state to feed into <see cref="MonorailCssGenerator.SeedCache"/>. The build task
/// constructs this from <c>obj/MonorailCss/*.json</c> at the start of a task invocation so
/// per-file source tokens and per-MVID assembly tokens carry over from the previous build.
/// </summary>
public sealed record GenerationCacheSeed(
    IReadOnlyList<SourceFileCacheEntry> SourceFiles,
    IReadOnlyList<AssemblyCacheEntry> Assemblies);

/// <summary>
/// Cache state returned by <see cref="MonorailCssGenerator.SnapshotCache"/> for persistence.
/// The build task serializes this to <c>obj/MonorailCss/*.json</c> after each generation so
/// the next process can avoid redoing work.
/// </summary>
public sealed record GenerationCacheSnapshot(
    IReadOnlyList<SourceFileCacheEntry> SourceFiles,
    IReadOnlyList<AssemblyCacheEntry> Assemblies);
