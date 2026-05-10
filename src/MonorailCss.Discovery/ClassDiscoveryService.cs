using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonorailCss.Parser.SourceCss;

namespace MonorailCss.Discovery;

/// <summary>
/// Orchestrates assembly scanning. Runs a full scan at startup, listens for newly-loaded
/// assemblies, and re-scans changed assemblies when hot-reload deltas land via
/// <see cref="MonorailCssReloader"/>. Maintains a cached CSS string + ETag served by
/// <see cref="MonorailCssMiddleware"/>.
/// </summary>
internal sealed class ClassDiscoveryService : IHostedService, IClassRegistry, IDisposable
{
    private const string SourceWatcherKey = "<source-watcher>";

    private static readonly string[] SourceFileFilters = ["*.razor", "*.cshtml", "*.cs", "*.html"];

    private readonly MonorailDiscoveryOptions _options;
    private readonly ILogger<ClassDiscoveryService> _logger;
    private readonly IHostEnvironment? _environment;
    private readonly DebouncedFileWatcher _sourceWatcher;
    private readonly DebouncedFileWatcher _cssWatcher;
    private readonly object _lock = new();
    private readonly HashSet<string> _pendingFiles = new(StringComparer.OrdinalIgnoreCase);

    // Per-assembly contributed classes; lets us answer "what came from where?" and rescan one
    // assembly without losing the others' contributions.
    private readonly Dictionary<string, ImmutableSortedSet<string>> _byAssembly = new(StringComparer.OrdinalIgnoreCase);

    // Mutable: rebuilt whenever the source CSS changes (the framework instance changes too).
    private ValidationCache _validationCache;
    private AssemblyClassScanner _scanner;
    private SourceFileScanner _sourceScanner;

    // Pass-through CSS (font-faces, keyframes, plain rules, layer-base content) prepended to
    // generated utilities at emit time. Populated by ProcessSourceCss; empty when no source CSS.
    private string _rawCss = string.Empty;

    // Files contributing to the source-CSS pipeline (entry file + every transitively imported).
    // Watched in development for hot-reload.
    private ImmutableList<string> _importedCssFiles = ImmutableList<string>.Empty;

    private ImmutableSortedSet<string> _classes = ImmutableSortedSet<string>.Empty;
    private string _generatedCss = string.Empty;
    private string _eTag = "\"empty\"";
    private DateTime _lastRegeneratedAt = DateTime.UtcNow;
    private int _regenerateCount;
    private int _hotReloadCount;
    private bool _started;

    public ClassDiscoveryService(
        IOptions<MonorailDiscoveryOptions> options,
        ILogger<ClassDiscoveryService> logger,
        IHostEnvironment? environment = null)
    {
        _options = options.Value;
        _logger = logger;
        _environment = environment;
        _validationCache = new ValidationCache(_options.Framework);
        _scanner = new AssemblyClassScanner(_validationCache);
        _sourceScanner = new SourceFileScanner(_validationCache);
        _sourceWatcher = new DebouncedFileWatcher(OnSourceDebounceTick);
        _cssWatcher = new DebouncedFileWatcher(OnCssDebounceTick);
    }

    public IReadOnlyCollection<string> GetClasses()
    {
        lock (_lock)
        {
            return _classes;
        }
    }

    public string Version
    {
        get
        {
            lock (_lock)
            {
                return _eTag;
            }
        }
    }

    public string Css
    {
        get
        {
            lock (_lock)
            {
                return _generatedCss;
            }
        }
    }

    public DiagnosticsSnapshot GetDiagnostics()
    {
        lock (_lock)
        {
            var perAssembly = _byAssembly
                .Select(kv => new AssemblyDiagnostics(kv.Key, kv.Value.Count, kv.Value.Take(10).ToArray()))
                .OrderByDescending(a => a.ClassCount)
                .ToArray();

            return new DiagnosticsSnapshot(
                ETag: _eTag,
                ClassCount: _classes.Count,
                CssLength: _generatedCss.Length,
                LastRegeneratedAt: _lastRegeneratedAt,
                RegenerateCount: _regenerateCount,
                HotReloadCount: _hotReloadCount,
                MvidCacheHits: _scanner.MvidCacheHits,
                MvidCacheMisses: _scanner.MvidCacheMisses,
                ValidationCacheSize: _validationCache.Count,
                Assemblies: perAssembly,
                AllClasses: _classes.ToArray());
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        MonorailCssReloader.RegisterService(this);

        ApplyEnvironmentDefaults();
        ProcessSourceCss(trigger: "startup");
        ForceLoadEntryAssemblyReferences();
        AutoDetectSourceRoots();

        var sw = Stopwatch.StartNew();
        var (scanned, skipped) = ScanAllLoadedAssemblies();
        ScanWatchedSources();
        Regenerate("startup");
        sw.Stop();

        StartFileWatchers();
        StartCssFileWatchers();

        _started = true;
        _logger.LogInformation(
            "MonorailCss discovery startup: {ClassCount} classes from {ScannedCount} assemblies (skipped {SkippedCount}) in {ElapsedMs} ms — ETag {Etag}",
            _classes.Count, scanned, skipped, sw.ElapsedMilliseconds, _eTag);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Wires up sensible defaults from <see cref="IHostEnvironment"/> so a bare
    /// <c>services.AddMonorailCss()</c> call works for typical apps:
    /// <list type="bullet">
    ///   <item>Watches <c>ContentRootPath</c> for source changes in development.</item>
    ///   <item>Sets <see cref="MonorailDiscoveryOptions.SourceCssPath"/> to <c>wwwroot/app.css</c> when present.</item>
    /// </list>
    /// Anything the user already configured wins; we only fill in blanks.
    /// </summary>
    private void ApplyEnvironmentDefaults()
    {
        if (_environment is null)
        {
            return;
        }

        if (_options.WatchSourceDirectories.Count == 0
            && _environment.IsDevelopment()
            && Directory.Exists(_environment.ContentRootPath))
        {
            _options.WatchSourceDirectories.Add(_environment.ContentRootPath);
            _logger.LogDebug("MonorailCss discovery: auto-watching {Dir}", _environment.ContentRootPath);
        }

        if (_options.SourceCss is null && _options.SourceCssPath is null)
        {
            var candidate = Path.Combine(_environment.ContentRootPath, "wwwroot", "app.css");
            if (File.Exists(candidate))
            {
                _options.SourceCssPath = candidate;
                _logger.LogDebug("MonorailCss discovery: auto-detected source CSS at {Path}", candidate);
            }
        }
    }

    /// <summary>
    /// Walks loaded assemblies and adds the local source directory of any whose PDB resolves
    /// to existing files on disk to <see cref="MonorailDiscoveryOptions.WatchSourceDirectories"/>.
    /// This bridges the common multi-project layout (host project references library projects
    /// that own the razor markup) without per-consumer configuration. Only runs in development;
    /// production deployments don't ship PDBs and don't watch.
    /// </summary>
    private void AutoDetectSourceRoots()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!ShouldScan(asm))
            {
                continue;
            }

            TryAutoWatchSourceRoot(asm, "auto-watching", out _);
        }
    }

    /// <summary>
    /// Resolves <paramref name="asm"/>'s local source root via its PDB and adds it to
    /// <see cref="MonorailDiscoveryOptions.WatchSourceDirectories"/> if it isn't tracked already.
    /// Returns true (and sets <paramref name="normalized"/>) only when a new directory was added.
    /// Skips silently in non-development environments.
    /// </summary>
    private bool TryAutoWatchSourceRoot(Assembly asm, string logPhrase, out string normalized)
    {
        normalized = string.Empty;

        if (_environment is { } env && !env.IsDevelopment())
        {
            return false;
        }

        if (!PdbSourceLocator.TryGetSourceRoot(asm, out var root))
        {
            return false;
        }

        normalized = NormalizeDirectory(root);
        foreach (var existing in _options.WatchSourceDirectories)
        {
            if (string.Equals(NormalizeDirectory(existing), normalized, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        _options.WatchSourceDirectories.Add(normalized);
        _logger.LogInformation(
            "MonorailCss discovery: {Phrase} {Dir} (from {Assembly})",
            logPhrase, normalized, asm.GetName().Name);
        return true;
    }

    private static string NormalizeDirectory(string dir)
    {
        try
        {
            return Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return dir;
        }
    }

    /// <summary>
    /// Runs the user's source CSS through <see cref="CssSourceProcessor"/>, replaces
    /// <see cref="MonorailDiscoveryOptions.Framework"/> with one configured from the parsed
    /// theme/applies/utilities/variants, and stores the residue raw CSS for verbatim emission.
    /// Re-runs on hot-reload of any imported file.
    /// </summary>
    private void ProcessSourceCss(string trigger)
    {
        if (string.IsNullOrWhiteSpace(_options.SourceCss) && string.IsNullOrWhiteSpace(_options.SourceCssPath))
        {
            return;
        }

        try
        {
            var sw = Stopwatch.StartNew();
            var processor = new CssSourceProcessor(msg => _logger.LogDebug("source-css: {Message}", msg));
            var baseSettings = _options.Framework.Settings;

            CssSourceResult result;
            if (!string.IsNullOrWhiteSpace(_options.SourceCssPath))
            {
                result = processor.ProcessFile(_options.SourceCssPath!, baseSettings);

                if (!string.IsNullOrWhiteSpace(_options.SourceCss))
                {
                    // Both set: layer the inline content on top of the file-derived settings.
                    var basePath = Path.GetDirectoryName(_options.SourceCssPath);
                    result = processor.ProcessSource(_options.SourceCss!, basePath, result.Settings);
                }
            }
            else
            {
                result = processor.ProcessSource(_options.SourceCss!, basePath: null, baseSettings);
            }

            // Replace the framework with one configured from the parsed CSS. The validation
            // cache and scanners hold a reference to the framework's TryValidateCandidate, so
            // they have to be rebuilt too.
            _options.Framework = new CssFramework(result.Settings);
            _validationCache = new ValidationCache(_options.Framework);
            _scanner = new AssemblyClassScanner(_validationCache);
            _sourceScanner = new SourceFileScanner(_validationCache);

            _rawCss = result.RawCss;
            _importedCssFiles = result.ImportedFiles;

            sw.Stop();
            _logger.LogInformation(
                "MonorailCss discovery: source CSS processed (trigger={Trigger}, files={FileCount}, applies={ApplyCount}, custom-utilities={UtilityCount}, custom-variants={VariantCount}, raw-residue={RawLen} chars, in {ElapsedMs} ms)",
                trigger,
                _importedCssFiles.Count,
                result.Settings.Applies.Count,
                result.Settings.CustomUtilities.Count,
                result.Settings.CustomVariants.Count,
                _rawCss.Length,
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MonorailCss discovery: failed to process source CSS — falling back to existing framework configuration");
        }
    }

    /// <summary>
    /// Walks the entry assembly's transitive references and force-loads each one (skipping the
    /// BCL/framework set). This guarantees component packages like Pennington.UI are present
    /// in <see cref="AppDomain.CurrentDomain"/> by the time the IL scanner runs, even if the
    /// host (e.g. ASP.NET Core, Blazor) hasn't touched a type from them yet.
    /// </summary>
    private void ForceLoadEntryAssemblyReferences()
    {
        var entry = Assembly.GetEntryAssembly();
        if (entry is null)
        {
            return;
        }

        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetName().Name ?? string.Empty)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var reference in entry.GetReferencedAssemblies())
        {
            var name = reference.Name ?? string.Empty;
            if (loaded.Contains(name) || IsKnownFrameworkAssembly(name) || _options.ExcludeAssemblies.Contains(name))
            {
                continue;
            }

            try
            {
                Assembly.Load(reference);
            }
            catch (Exception ex)
            {
                _logger.LogTrace(ex, "MonorailCss discovery: skipped reference {Name} (load failed)", name);
            }
        }
    }

    private static bool IsKnownFrameworkAssembly(string name)
    {
        // Skip BCL / runtime libraries — they're full of strings the parser would have to chew
        // through and the false-positive rate is essentially zero in practice. Anything else
        // (third-party packages, the user's own assemblies, RCLs) gets scanned.
        return name.StartsWith("System.", StringComparison.Ordinal)
            || name.StartsWith("Microsoft.", StringComparison.Ordinal)
            || name.StartsWith("netstandard", StringComparison.Ordinal)
            || name.Equals("mscorlib", StringComparison.Ordinal)
            || name.Equals("WindowsBase", StringComparison.Ordinal);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
        MonorailCssReloader.UnregisterService(this);
        _sourceWatcher.Stop();
        _cssWatcher.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
        MonorailCssReloader.UnregisterService(this);
        _sourceWatcher.Dispose();
        _cssWatcher.Dispose();
    }

    private void StartFileWatchers()
    {
        foreach (var dir in _options.WatchSourceDirectories)
        {
            AddWatcherFor(dir);
        }
    }

    private void AddWatcherFor(string dir)
    {
        if (!Directory.Exists(dir))
        {
            _logger.LogWarning("MonorailCss discovery: WatchSourceDirectories entry {Dir} does not exist", dir);
            return;
        }

        if (_sourceWatcher.AddDirectory(dir, SourceFileFilters, includeSubdirectories: true, onChange: EnqueuePendingSourceFile))
        {
            _logger.LogInformation("MonorailCss discovery: watching source directory {Dir}", dir);
        }
    }

    /// <summary>
    /// Watches every file the source-CSS pipeline pulled in (entry path + every transitively
    /// imported file). Edits to any of them re-run <see cref="ProcessSourceCss"/>, rebuild the
    /// framework, and regenerate. We use a per-directory <see cref="DebouncedFileWatcher"/> with
    /// a filename filter so cross-directory imports (e.g. NuGet-shipped theme files under
    /// <c>_content/</c>) work too.
    /// </summary>
    private void StartCssFileWatchers()
    {
        if (_importedCssFiles.Count == 0)
        {
            return;
        }

        if (_environment is { } env && !env.IsDevelopment())
        {
            return;
        }

        var byDirectory = _importedCssFiles
            .Where(p => !string.IsNullOrEmpty(p))
            .GroupBy(p => Path.GetDirectoryName(p) ?? string.Empty, StringComparer.OrdinalIgnoreCase);

        foreach (var group in byDirectory)
        {
            var dir = group.Key;
            if (string.IsNullOrEmpty(dir))
            {
                continue;
            }

            var filenames = group
                .Select(Path.GetFileName)
                .Where(n => !string.IsNullOrEmpty(n))
                .Cast<string>()
                .ToArray();

            if (_cssWatcher.AddDirectory(dir, filenames, includeSubdirectories: false))
            {
                _logger.LogDebug("MonorailCss discovery: watching CSS in {Dir} ({Count} files)", dir, filenames.Length);
            }
        }
    }

    private void EnqueuePendingSourceFile(string path)
    {
        lock (_pendingFiles)
        {
            _pendingFiles.Add(path);
        }
    }

    private void OnCssDebounceTick()
    {
        _logger.LogInformation("MonorailCss discovery: source CSS changed — re-processing");
        ProcessSourceCss(trigger: "css-watcher");

        // The set of imported files may have shifted (e.g. a new @import landed). Reset the
        // CSS watchers and rebuild assembly contributions through the (potentially) new
        // validation cache.
        _cssWatcher.Stop();
        StartCssFileWatchers();

        ScanAllLoadedAssemblies();
        ScanWatchedSources();
        Regenerate("css-watcher");
    }

    private void OnSourceDebounceTick()
    {
        string[] paths;
        lock (_pendingFiles)
        {
            paths = _pendingFiles.ToArray();
            _pendingFiles.Clear();
        }

        if (paths.Length == 0)
        {
            return;
        }

        _logger.LogInformation("MonorailCss discovery: source file change ({Count} files): {Files}", paths.Length, string.Join(", ", paths.Select(Path.GetFileName)));

        // Rescan all watched directories' source files. Cheaper than tracking per-file deltas
        // and avoids losing a class when one file removes it but another still uses it.
        ScanWatchedSources();
        Regenerate("source-watcher");
    }

    private void ScanWatchedSources()
    {
        if (_options.WatchSourceDirectories.Count == 0)
        {
            return;
        }

        var bucket = new HashSet<string>();
        var fileCount = 0;
        foreach (var dir in _options.WatchSourceDirectories)
        {
            if (!Directory.Exists(dir))
            {
                continue;
            }

            foreach (var file in EnumerateSourceFiles(dir))
            {
                _sourceScanner.ScanFile(file, bucket);
                fileCount++;
            }
        }

        if (bucket.Count == 0)
        {
            return;
        }

        var contributed = bucket.ToImmutableSortedSet(StringComparer.Ordinal);
        lock (_lock)
        {
            _byAssembly[SourceWatcherKey] = contributed;
            RebuildClasses();
        }

        _logger.LogDebug("MonorailCss discovery: source-watcher scanned {FileCount} files, {Count} candidate classes", fileCount, contributed.Count);
    }

    /// <summary>
    /// Rebuilds <see cref="_classes"/> from the current <see cref="_byAssembly"/> contributions
    /// plus the user's extra safelist. Caller must hold <see cref="_lock"/>.
    /// </summary>
    private void RebuildClasses()
    {
        _classes = _byAssembly.Values
            .SelectMany(s => s)
            .Concat(_options.ExtraSafelist)
            .ToImmutableSortedSet(StringComparer.Ordinal);
    }

    private static IEnumerable<string> EnumerateSourceFiles(string dir)
    {
        return Directory.EnumerateFiles(dir, "*.razor", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(dir, "*.cshtml", SearchOption.AllDirectories))
            .Concat(Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories))
            .Concat(Directory.EnumerateFiles(dir, "*.html", SearchOption.AllDirectories))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                     && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
    }

    /// <summary>
    /// Re-scans the supplied assemblies (a hot-reload delta). Called by the metadata-update
    /// handler. Invalidates the IL-scan cache for each named assembly before rescanning so
    /// stale entries from before the delta don't short-circuit the walk. In practice the
    /// source-file watcher path is what surfaces in-session razor edits, since
    /// <c>Assembly.TryGetRawMetadata</c> returns the base PE image and EnC <c>#US</c>
    /// additions live in deltas the IL scanner doesn't read; the invalidation here is
    /// defensive cover for runtime paths that swap the in-memory metadata image.
    /// </summary>
    internal void OnAssembliesChanged(IEnumerable<Assembly> changed)
    {
        Interlocked.Increment(ref _hotReloadCount);
        var changedList = changed.ToArray();

        _logger.LogInformation(
            "MonorailCss discovery: hot-reload event #{Count} — {ChangedCount} changed assemblies: {Names}",
            _hotReloadCount,
            changedList.Length,
            string.Join(", ", changedList.Select(a => a.GetName().Name)));

        foreach (var asm in changedList)
        {
            try
            {
                _scanner.Invalidate(asm.ManifestModule.ModuleVersionId);
            }
            catch
            {
                // Some dynamic / collectible assemblies don't expose a stable MVID; nothing to
                // invalidate in that case.
            }
        }

        // Re-scan every loaded assembly, not just changed ones. The per-assembly cache lets us
        // detect which assembly's contribution actually changed, but the metadata-image walk is
        // cheap enough that re-scanning all is simpler and still fast.
        var (scanned, skipped) = ScanAllLoadedAssemblies();
        if (scanned == 0)
        {
            _logger.LogWarning("MonorailCss discovery: hot-reload triggered but zero assemblies accepted for scan");
            return;
        }

        Regenerate("hot-reload");
        _logger.LogInformation(
            "MonorailCss discovery: hot-reload rescan — {ClassCount} classes total, {ScannedCount} scanned, {SkippedCount} skipped, ETag {Etag}",
            _classes.Count, scanned, skipped, _eTag);
    }

    private (int Scanned, int Skipped) ScanAllLoadedAssemblies()
    {
        var scanned = 0;
        var skipped = 0;
        var newByAssembly = new Dictionary<string, ImmutableSortedSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = asm.GetName().Name ?? "<unnamed>";

            if (!ShouldScan(asm))
            {
                skipped++;
                continue;
            }

            var bucket = new HashSet<string>();
            var accepted = _scanner.Scan(asm, bucket);
            if (!accepted)
            {
                skipped++;
                _logger.LogTrace("MonorailCss discovery: {Name} rejected (no metadata or reference-only)", name);
                continue;
            }

            scanned++;
            if (bucket.Count > 0)
            {
                newByAssembly[name] = bucket.ToImmutableSortedSet(StringComparer.Ordinal);
                _logger.LogDebug("MonorailCss discovery: {Name} contributed {Count} classes", name, bucket.Count);
            }
        }

        lock (_lock)
        {
            // Drop only keys we own (assemblies). Other contributors — most importantly
            // <source-watcher> — survive an IL rescan, otherwise hot-reload events would
            // clobber the source-discovered classes added moments earlier when the same
            // file change triggered the FileSystemWatcher.
            var assemblyKeys = _byAssembly.Keys.Where(k => !k.StartsWith('<')).ToArray();
            foreach (var k in assemblyKeys)
            {
                _byAssembly.Remove(k);
            }

            foreach (var (k, v) in newByAssembly)
            {
                _byAssembly[k] = v;
            }

            RebuildClasses();
        }

        return (scanned, skipped);
    }

    private void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
    {
        var asm = args.LoadedAssembly;
        var name = asm.GetName().Name ?? "<unnamed>";

        if (!ShouldScan(asm))
        {
            return;
        }

        var bucket = new HashSet<string>();
        if (!_scanner.Scan(asm, bucket))
        {
            return;
        }

        if (bucket.Count == 0)
        {
            _logger.LogDebug("MonorailCss discovery: late-load {Name} accepted but contributed 0 classes", name);
            return;
        }

        var contributed = bucket.ToImmutableSortedSet(StringComparer.Ordinal);
        bool changed;
        lock (_lock)
        {
            if (_byAssembly.TryGetValue(name, out var existing) && existing.SetEquals(contributed))
            {
                changed = false;
            }
            else
            {
                _byAssembly[name] = contributed;
                RebuildClasses();
                changed = true;
            }
        }

        if (changed && _started)
        {
            _logger.LogInformation("MonorailCss discovery: late-load {Name} added {Count} classes", name, contributed.Count);
            Regenerate("late-load");
        }

        TryAddSourceRootForLateLoad(asm);
    }

    private void TryAddSourceRootForLateLoad(Assembly asm)
    {
        if (!_started)
        {
            return;
        }

        if (!TryAutoWatchSourceRoot(asm, "late-load auto-watching", out var normalized))
        {
            return;
        }

        AddWatcherFor(normalized);

        // Pull the new directory's source files into the contribution set immediately so the
        // late-loaded RCL's classes show up without waiting for an edit.
        ScanWatchedSources();
        Regenerate("late-load-source-root");
    }

    private bool ShouldScan(Assembly asm)
    {
        if (asm.IsDynamic)
        {
            return false;
        }

        var name = asm.GetName().Name;
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        // Skip the BCL noise — same filter we use during force-load. The IL scanner would
        // reject most of these strings anyway, but this avoids walking 100+ heaps for nothing.
        if (IsKnownFrameworkAssembly(name))
        {
            return false;
        }

        // User-configured exclusions: utility libraries (icon packs, the framework itself)
        // whose IL-embedded strings would inflate the discovered class set.
        return !_options.ExcludeAssemblies.Contains(name);
    }

    private void Regenerate(string trigger)
    {
        ImmutableSortedSet<string> snapshot;
        lock (_lock)
        {
            snapshot = _classes;
        }

        var sw = Stopwatch.StartNew();
        var generated = _options.Framework.Process(snapshot);

        var combined = string.IsNullOrWhiteSpace(_rawCss)
            ? generated
            : _rawCss + "\n\n" + generated;

        var etag = ComputeETag(combined);
        sw.Stop();

        lock (_lock)
        {
            _generatedCss = combined;
            _eTag = etag;
            _lastRegeneratedAt = DateTime.UtcNow;
            _regenerateCount++;
        }

        _logger.LogInformation(
            "MonorailCss discovery: regenerated CSS (trigger={Trigger}, classes={ClassCount}, length={Length}, etag={Etag}, in {ElapsedMs} ms)",
            trigger, snapshot.Count, combined.Length, etag, sw.ElapsedMilliseconds);

        if (!string.IsNullOrEmpty(_options.WriteToFile))
        {
            TryWriteToFile(combined);
        }
    }

    private void TryWriteToFile(string css)
    {
        try
        {
            var path = _options.WriteToFile!;
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var tmp = path + ".tmp";
            File.WriteAllText(tmp, css);
            File.Move(tmp, path, overwrite: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MonorailCss discovery: failed to write CSS to {Path}", _options.WriteToFile);
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

/// <summary>
/// Snapshot of the discovery service's current state. Returned by the diagnostics endpoint.
/// </summary>
public sealed record DiagnosticsSnapshot(
    string ETag,
    int ClassCount,
    int CssLength,
    DateTime LastRegeneratedAt,
    int RegenerateCount,
    int HotReloadCount,
    long MvidCacheHits,
    long MvidCacheMisses,
    int ValidationCacheSize,
    IReadOnlyList<AssemblyDiagnostics> Assemblies,
    IReadOnlyList<string> AllClasses);

/// <summary>
/// Per-assembly contribution to the global class set.
/// </summary>
public sealed record AssemblyDiagnostics(
    string Name,
    int ClassCount,
    IReadOnlyList<string> SampleClasses);
