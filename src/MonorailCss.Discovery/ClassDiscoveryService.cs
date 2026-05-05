using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

    private readonly MonorailDiscoveryOptions _options;
    private readonly ILogger<ClassDiscoveryService> _logger;
    private readonly IHostEnvironment? _environment;
    private readonly AssemblyClassScanner _scanner;
    private readonly SourceFileScanner _sourceScanner;
    private readonly ValidationCache _validationCache;
    private readonly List<FileSystemWatcher> _fileWatchers = new();
    private readonly object _lock = new();
    private readonly System.Threading.Timer _debounce;
    private readonly HashSet<string> _pendingFiles = new(StringComparer.OrdinalIgnoreCase);

    // Per-assembly contributed classes; lets us answer "what came from where?" and rescan one
    // assembly without losing the others' contributions.
    private readonly Dictionary<string, ImmutableSortedSet<string>> _byAssembly = new(StringComparer.OrdinalIgnoreCase);

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
        _scanner = new AssemblyClassScanner(_validationCache, new PreFilter(_options.Framework));
        _sourceScanner = new SourceFileScanner(_validationCache);
        _debounce = new System.Threading.Timer(OnDebounceTick, null, Timeout.Infinite, Timeout.Infinite);
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

    public string ETag
    {
        get
        {
            lock (_lock)
            {
                return _eTag;
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
        ForceLoadEntryAssemblyReferences();

        var sw = Stopwatch.StartNew();
        var (scanned, skipped) = ScanAllLoadedAssemblies();
        ScanWatchedSources();
        Regenerate("startup");
        sw.Stop();

        StartFileWatchers();

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
    ///   <item>Loads <c>wwwroot/app.css</c> as the source CSS prefix if it exists.</item>
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

        if (_options.SourceCss is null)
        {
            var candidate = Path.Combine(_environment.ContentRootPath, "wwwroot", "app.css");
            if (File.Exists(candidate))
            {
                try
                {
                    _options.SourceCss = File.ReadAllText(candidate);
                    _logger.LogDebug("MonorailCss discovery: auto-loaded source CSS from {Path}", candidate);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "MonorailCss discovery: could not read {Path}", candidate);
                }
            }
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
            if (loaded.Contains(name) || IsKnownFrameworkAssembly(name))
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
        StopFileWatchers();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
        MonorailCssReloader.UnregisterService(this);
        StopFileWatchers();
        _debounce.Dispose();
    }

    private void StartFileWatchers()
    {
        foreach (var dir in _options.WatchSourceDirectories)
        {
            if (!Directory.Exists(dir))
            {
                _logger.LogWarning("MonorailCss discovery: WatchSourceDirectories entry {Dir} does not exist", dir);
                continue;
            }

            var watcher = new FileSystemWatcher(dir)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                EnableRaisingEvents = true,
            };

            watcher.Filters.Add("*.razor");
            watcher.Filters.Add("*.cshtml");
            watcher.Filters.Add("*.cs");
            watcher.Filters.Add("*.html");

            watcher.Changed += OnSourceFileChanged;
            watcher.Created += OnSourceFileChanged;
            watcher.Renamed += OnSourceFileRenamed;

            _fileWatchers.Add(watcher);
            _logger.LogInformation("MonorailCss discovery: watching source directory {Dir}", dir);
        }
    }

    private void StopFileWatchers()
    {
        foreach (var w in _fileWatchers)
        {
            try
            {
                w.EnableRaisingEvents = false;
                w.Dispose();
            }
            catch
            {
                // best-effort
            }
        }

        _fileWatchers.Clear();
    }

    private void OnSourceFileChanged(object sender, FileSystemEventArgs e)
    {
        QueueSourceFile(e.FullPath);
    }

    private void OnSourceFileRenamed(object sender, RenamedEventArgs e)
    {
        QueueSourceFile(e.FullPath);
    }

    private void QueueSourceFile(string path)
    {
        lock (_pendingFiles)
        {
            _pendingFiles.Add(path);
        }

        // Coalesce bursts (editors often save twice) into a single rescan.
        _debounce.Change(150, Timeout.Infinite);
    }

    private void OnDebounceTick(object? state)
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
            _classes = _byAssembly.Values
                .SelectMany(s => s)
                .Concat(_options.ExtraSafelist)
                .ToImmutableSortedSet(StringComparer.Ordinal);
        }

        _logger.LogDebug("MonorailCss discovery: source-watcher scanned {FileCount} files, {Count} candidate classes", fileCount, contributed.Count);
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
    /// Re-scans the supplied assemblies (a hot-reload delta). Called by the metadata-update handler.
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

            _classes = _byAssembly.Values
                .SelectMany(s => s)
                .Concat(_options.ExtraSafelist)
                .ToImmutableSortedSet(StringComparer.Ordinal);
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
                _classes = _byAssembly.Values
                    .SelectMany(s => s)
                    .Concat(_options.ExtraSafelist)
                    .ToImmutableSortedSet(StringComparer.Ordinal);
                changed = true;
            }
        }

        if (changed && _started)
        {
            _logger.LogInformation("MonorailCss discovery: late-load {Name} added {Count} classes", name, contributed.Count);
            Regenerate("late-load");
        }
    }

    private static bool ShouldScan(Assembly asm)
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
        return !IsKnownFrameworkAssembly(name);
    }

    private void Regenerate(string trigger)
    {
        ImmutableSortedSet<string> snapshot;
        lock (_lock)
        {
            snapshot = _classes;
        }

        var sw = Stopwatch.StartNew();
        var classList = snapshot.Concat(_options.ExtraSafelist);
        var generated = _options.Framework.Process(classList);

        var combined = string.IsNullOrWhiteSpace(_options.SourceCss)
            ? generated
            : _options.SourceCss + "\n\n" + generated;

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
