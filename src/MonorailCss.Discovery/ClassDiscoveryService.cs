using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MonorailCss.Discovery;

/// <summary>
/// Hosts a <see cref="MonorailCssGenerator"/> for the runtime: forces transitive references
/// to load at startup, listens for newly-loaded assemblies, watches the source CSS + content
/// directories for hot-reload, and re-runs the generator whenever any of those change.
/// Maintains the cached <see cref="MonorailCssGenerationResult"/> served by
/// <see cref="MonorailCssMiddleware"/>.
/// </summary>
internal sealed class ClassDiscoveryService : IHostedService, IClassRegistry, IDisposable
{
    private readonly MonorailDiscoveryOptions _options;
    private readonly ILogger<ClassDiscoveryService> _logger;
    private readonly IHostEnvironment? _environment;
    private readonly MonorailCssGenerator _generator = new();
    private readonly List<FileSystemWatcher> _fileWatchers = new();
    private readonly List<FileSystemWatcher> _cssWatchers = new();
    private readonly object _lock = new();
    private readonly System.Threading.Timer _debounce;
    private readonly System.Threading.Timer _cssDebounce;
    private readonly HashSet<string> _pendingFiles = new(StringComparer.OrdinalIgnoreCase);

    private MonorailCssGenerationResult _result;
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
        _result = new MonorailCssGenerationResult(
            Css: string.Empty,
            ETag: "\"empty\"",
            Classes: ImmutableSortedSet<string>.Empty,
            ImportedCssFiles: ImmutableList<string>.Empty,
            Framework: _options.Framework,
            SourceConfiguration: new MonorailCss.Parser.SourceCss.SourceConfiguration());
        _debounce = new System.Threading.Timer(OnDebounceTick, null, Timeout.Infinite, Timeout.Infinite);
        _cssDebounce = new System.Threading.Timer(OnCssDebounceTick, null, Timeout.Infinite, Timeout.Infinite);
    }

    public IReadOnlyCollection<string> GetClasses()
    {
        lock (_lock)
        {
            return _result.Classes;
        }
    }

    public string Version
    {
        get
        {
            lock (_lock)
            {
                return _result.ETag;
            }
        }
    }

    public string Css
    {
        get
        {
            lock (_lock)
            {
                return _result.Css;
            }
        }
    }

    public DiagnosticsSnapshot GetDiagnostics()
    {
        lock (_lock)
        {
            return new DiagnosticsSnapshot(
                ETag: _result.ETag,
                ClassCount: _result.Classes.Count,
                CssLength: _result.Css.Length,
                LastRegeneratedAt: _lastRegeneratedAt,
                RegenerateCount: _regenerateCount,
                HotReloadCount: _hotReloadCount,
                ImportedCssFileCount: _result.ImportedCssFiles.Count,
                AllClasses: _result.Classes.ToArray());
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        MonorailCssReloader.RegisterService(this);

        ApplyEnvironmentDefaults();
        ForceLoadEntryAssemblyReferences();

        var sw = Stopwatch.StartNew();
        Regenerate("startup");
        sw.Stop();

        StartFileWatchers();
        StartCssFileWatchers();

        _started = true;
        _logger.LogInformation(
            "MonorailCss discovery startup: {ClassCount} classes in {ElapsedMs} ms — ETag {Etag}",
            _result.Classes.Count, sw.ElapsedMilliseconds, _result.ETag);
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
    /// Walks the entry assembly's transitive references and force-loads each one (skipping the
    /// BCL/framework set). This guarantees component packages like Pennington.UI are present
    /// in <see cref="AppDomain.CurrentDomain"/> by the time the generator runs, even if the
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
            if (loaded.Contains(name)
                || IlMetadataScanner.IsKnownFrameworkAssembly(name)
                || _options.ExcludeAssemblies.Contains(name))
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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
        MonorailCssReloader.UnregisterService(this);
        StopFileWatchers();
        StopCssFileWatchers();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
        MonorailCssReloader.UnregisterService(this);
        StopFileWatchers();
        StopCssFileWatchers();
        _debounce.Dispose();
        _cssDebounce.Dispose();
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

    /// <summary>
    /// Watches every file the source-CSS pipeline pulled in (entry path + every transitively
    /// imported file). Edits to any of them trigger a regeneration. We use a per-directory
    /// <see cref="FileSystemWatcher"/> with a filename filter so cross-directory imports
    /// (e.g. NuGet-shipped theme files under <c>_content/</c>) work too.
    /// </summary>
    private void StartCssFileWatchers()
    {
        if (_result.ImportedCssFiles.Count == 0)
        {
            return;
        }

        if (_environment is { } env && !env.IsDevelopment())
        {
            return;
        }

        var byDirectory = _result.ImportedCssFiles
            .Where(p => !string.IsNullOrEmpty(p))
            .GroupBy(p => Path.GetDirectoryName(p) ?? string.Empty, StringComparer.OrdinalIgnoreCase);

        foreach (var group in byDirectory)
        {
            var dir = group.Key;
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                continue;
            }

            var watcher = new FileSystemWatcher(dir)
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                EnableRaisingEvents = true,
            };

            foreach (var file in group)
            {
                watcher.Filters.Add(Path.GetFileName(file));
            }

            watcher.Changed += OnCssFileChanged;
            watcher.Created += OnCssFileChanged;
            watcher.Renamed += OnCssFileRenamed;

            _cssWatchers.Add(watcher);
            _logger.LogDebug("MonorailCss discovery: watching CSS in {Dir} ({Count} files)", dir, group.Count());
        }
    }

    private void StopCssFileWatchers()
    {
        foreach (var w in _cssWatchers)
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

        _cssWatchers.Clear();
    }

    private void OnCssFileChanged(object sender, FileSystemEventArgs e) => QueueCssReload();

    private void OnCssFileRenamed(object sender, RenamedEventArgs e) => QueueCssReload();

    private void QueueCssReload()
    {
        // Coalesce bursts (editors often save twice + the import processor's output triggers a
        // self-write when WriteToFile is set) into a single re-process.
        _cssDebounce.Change(150, Timeout.Infinite);
    }

    private void OnCssDebounceTick(object? state)
    {
        _logger.LogInformation("MonorailCss discovery: source CSS changed — re-processing");
        Regenerate("css-watcher");

        // Imported file set may have shifted (a new @import landed, a removed one disappeared).
        StopCssFileWatchers();
        StartCssFileWatchers();
    }

    private void OnSourceFileChanged(object sender, FileSystemEventArgs e) => QueueSourceFile(e.FullPath);

    private void OnSourceFileRenamed(object sender, RenamedEventArgs e) => QueueSourceFile(e.FullPath);

    private void QueueSourceFile(string path)
    {
        lock (_pendingFiles)
        {
            _pendingFiles.Add(path);
        }

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

        _logger.LogInformation(
            "MonorailCss discovery: source file change ({Count} files): {Files}",
            paths.Length,
            string.Join(", ", paths.Select(Path.GetFileName)));

        Regenerate("source-watcher");
    }

    /// <summary>
    /// Re-scans every loaded assembly when the runtime applies a hot-reload delta. Called by
    /// <see cref="MonorailCssReloader.UpdateApplication"/>; the changed-assembly list is
    /// informational because the generator's MVID cache short-circuits unchanged assemblies
    /// anyway.
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

        Regenerate("hot-reload");
    }

    private void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
    {
        if (!_started)
        {
            return;
        }

        // Late-loaded assembly. The generator will pick it up on its next regeneration via the
        // AppDomain.GetAssemblies() snapshot, and its MVID cache means the rescan cost for the
        // already-known assemblies is negligible.
        Regenerate("late-load");
    }

    private void Regenerate(string trigger)
    {
        var sw = Stopwatch.StartNew();
        var result = _generator.Generate(new MonorailCssGenerationRequest
        {
            SourceCss = _options.SourceCss,
            SourceCssPath = _options.SourceCssPath,
            BaseFramework = _options.Framework,
            Assemblies = AppDomain.CurrentDomain.GetAssemblies(),
            SourceFiles = EnumerateWatchedSourceFiles(),
            ExtraSafelist = _options.ExtraSafelist,
            ExcludeAssemblies = _options.ExcludeAssemblies,
        });
        sw.Stop();

        lock (_lock)
        {
            _result = result;
            _lastRegeneratedAt = DateTime.UtcNow;
            _regenerateCount++;
            // Surface the generator-built framework so options consumers can read it back.
            _options.Framework = result.Framework;
        }

        _logger.LogInformation(
            "MonorailCss discovery: regenerated CSS (trigger={Trigger}, classes={ClassCount}, length={Length}, etag={Etag}, in {ElapsedMs} ms)",
            trigger, result.Classes.Count, result.Css.Length, result.ETag, sw.ElapsedMilliseconds);

        if (!string.IsNullOrEmpty(_options.WriteToFile))
        {
            TryWriteToFile(result.Css);
        }
    }

    private List<string> EnumerateWatchedSourceFiles()
    {
        var files = new List<string>();
        foreach (var dir in _options.WatchSourceDirectories)
        {
            if (!Directory.Exists(dir))
            {
                continue;
            }

            files.AddRange(EnumerateSourceFiles(dir));
        }

        return files;
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
    int ImportedCssFileCount,
    IReadOnlyList<string> AllClasses);
