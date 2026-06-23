using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
internal sealed partial class ClassDiscoveryService : IHostedService, IClassRegistry, IDisposable
{
    private readonly MonorailDiscoveryOptions _options;
    private readonly ILogger<ClassDiscoveryService> _logger;
    private readonly IHostEnvironment? _environment;
    private readonly MonorailCssGenerator _generator = new();
    private readonly List<FileSystemWatcher> _fileWatchers = new();
    private readonly List<FileSystemWatcher> _cssWatchers = new();
    private readonly Lock _lock = new();

    // Guards every read/write of the watcher lists above. They are mutated from at least three
    // places that can overlap: StartAsync, the CSS-debounce timer callback (which restarts the
    // CSS watchers), and teardown — where StopAsync and Dispose can both run StopFileWatchers,
    // one thread's Clear() invalidating the other's enumeration. Serialize them all here.
    private readonly Lock _watcherLock = new();
    private readonly Timer _debounce;
    private readonly Timer _cssDebounce;
    private readonly Timer _lateLoadDebounce;
    private readonly HashSet<string> _pendingFiles = new(StringComparer.OrdinalIgnoreCase);
    private bool _pendingFullRescan;

    // Set once during teardown (StopAsync/Dispose) under _watcherLock so the shared teardown runs
    // exactly once and a late CSS-debounce tick can't resurrect watchers after shutdown.
    private bool _disposed;

    private MonorailCssGenerationResult _result;
    private DateTime _lastRegeneratedAt = DateTime.UtcNow;
    private int _regenerateCount;
    private int _hotReloadCount;
    private bool _started;
    private List<string>? _staticWebAssetFiles;

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
            SourceConfiguration: new Parser.SourceCss.SourceConfiguration());
        _debounce = new Timer(OnDebounceTick, null, Timeout.Infinite, Timeout.Infinite);
        _cssDebounce = new Timer(OnCssDebounceTick, null, Timeout.Infinite, Timeout.Infinite);
        _lateLoadDebounce = new Timer(OnLateLoadDebounceTick, null, Timeout.Infinite, Timeout.Infinite);
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
                WatchSourceDirectories: _options.WatchSourceDirectories.ToArray(),
                AllClasses: _result.Classes.ToArray());
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        MonorailCssReloader.RegisterService(this);

        ApplyEnvironmentDefaults();
        ForceLoadEntryAssemblyReferences();

        // Force-load runs first so every referenced project assembly is present before we read
        // PDBs to locate their source directories (the dotnet watch cross-project case). Adding
        // the roots before the startup scan lets that first scan walk them too.
        AddReferencedProjectWatchRoots();

        var sw = Stopwatch.StartNew();
        Regenerate("startup");
        sw.Stop();

        StartFileWatchers();
        StartCssFileWatchers();

        _started = true;
        LogMonorailcssDiscoveryStartupClassCloutElapsedMsEtag(_result.Classes.Count, sw.ElapsedMilliseconds, _result.ETag);
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
            LogMonorailcssDiscoveryAutoWatchingDir(_environment.ContentRootPath);
        }

        if (_options.SourceCss is null && _options.SourceCssPath is null)
        {
            var candidate = Path.Combine(_environment.ContentRootPath, "wwwroot", "app.css");
            if (File.Exists(candidate))
            {
                _options.SourceCssPath = candidate;
                LogMonorailcssDiscoveryAutoDetectedSourceCssAtPath(candidate);
            }
        }
    }

    /// <summary>
    /// Walks the entry assembly's transitive references and force-loads each one (skipping the
    /// BCL/framework set). This guarantees component packages like Pennington.UI are present
    /// in <see cref="AppDomain.CurrentDomain"/> by the time the generator runs, even if the
    /// host (e.g. ASP.NET Core, Blazor) hasn't touched a type from them yet.
    /// </summary>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' may break when trimming",
        Justification = "Runtime class discovery is inherently a reflection feature: it walks the entry assembly's reference graph to force-load component packages. Under trimming the graph may be incomplete, in which case discovery simply finds fewer classes — the load is best-effort and guarded. AddMonorailClassDiscovery documents that this feature is unsuitable for fully-trimmed/Native-AOT deployments.")]
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
                LogMonorailcssDiscoverySkippedReferenceNameLoadFailed(name, ex);
            }
        }
    }

    /// <summary>
    /// When running under <c>dotnet watch</c> (or when explicitly enabled via
    /// <see cref="MonorailDiscoveryOptions.WatchReferencedProjectSources"/>), discovers the source
    /// directories of referenced, locally-built projects from their PDBs and adds each — once,
    /// deduplicated against the directories already being watched — to the watch set. This is what
    /// lets an edit to a <c>.razor</c>/<c>.cs</c> file in a referenced library (whose source lives
    /// outside the app's content root) trigger regeneration. Gated behind <c>DOTNET_WATCH</c> by
    /// default because cross-project source watching is only meaningful — and only safe from
    /// surprising production behavior — under the watch host.
    /// </summary>
    private void AddReferencedProjectWatchRoots()
    {
        if (!ShouldWatchReferencedProjectSources())
        {
            return;
        }

        var roots = ProjectSourceRootResolver.ResolveWatchRoots(
            AppDomain.CurrentDomain.GetAssemblies(),
            _options.ExcludeAssemblies);

        var added = 0;
        foreach (var root in roots)
        {
            // Store the canonical absolute form so the watcher and dedup compare consistently.
            var normalized = NormalizeDirectory(root);
            if (OverlapsExistingWatch(normalized))
            {
                continue;
            }

            _options.WatchSourceDirectories.Add(normalized);
            added++;
            LogMonorailcssDiscoveryWatchingReferencedProjectSource(normalized);
        }

        LogMonorailcssDiscoveryReferencedProjectRootsDiscovered(roots.Count, added);
    }

    /// <summary>
    /// Resolves the effective referenced-project watching mode: an explicit option value wins,
    /// otherwise it's on iff the <c>DOTNET_WATCH</c> environment variable is present (set by
    /// <c>dotnet watch</c>).
    /// </summary>
    private bool ShouldWatchReferencedProjectSources()
    {
        return _options.WatchReferencedProjectSources
               ?? !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_WATCH"));
    }

    /// <summary>
    /// True when <paramref name="candidate"/> (already normalized) overlaps an existing watched
    /// directory in <em>either</em> direction — equal to it, nested beneath it, or a strict
    /// ancestor of it — so it should not be added. Equal/nested is simply redundant. The ancestor
    /// case matters more: a recursive <see cref="FileSystemWatcher"/> does not honor
    /// <see cref="DiscoveryPaths.IgnoredDirectoryNames"/> at the OS level, so adding a root that
    /// sits above an already-watched directory would escalate a narrow watch into a whole-parent-
    /// tree watch (pulling in <c>.git</c>/<c>node_modules</c>/sibling <c>bin</c>/<c>obj</c> and
    /// risking OS watch-handle limits). Only roots disjoint from every existing entry are added;
    /// in particular a referenced-project root that is an ancestor of the content root is rejected
    /// rather than broadening the app's own watch.
    /// </summary>
    private bool OverlapsExistingWatch(string candidate)
    {
        foreach (var existing in _options.WatchSourceDirectories)
        {
            var normalizedExisting = NormalizeDirectory(existing);
            if (IsSameOrUnder(candidate, normalizedExisting) || IsSameOrUnder(normalizedExisting, candidate))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeDirectory(string path)
    {
        try
        {
            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        }
        catch
        {
            return path;
        }
    }

    private static bool IsSameOrUnder(string child, string parent)
    {
        if (string.Equals(child, parent, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return child.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    // StopAsync (graceful host shutdown) and Dispose (container disposal) are distinct lifecycle
    // hooks: a clean shutdown fires both, but an abnormal one may fire only Dispose, so each must
    // fully tear the service down on its own. They share one idempotent Teardown so the work runs
    // exactly once regardless of order or which hook fires.
    public Task StopAsync(CancellationToken cancellationToken)
    {
        Teardown();
        return Task.CompletedTask;
    }

    public void Dispose() => Teardown();

    private void Teardown()
    {
        // _watcherLock serializes the whole teardown against the watcher Start/Stop paths (incl.
        // the CSS-debounce timer's restart) and makes the run-once guard atomic. None of the work
        // below acquires _lock, so holding _watcherLock here can't invert against the result lock.
        lock (_watcherLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
            MonorailCssReloader.UnregisterService(this);

            StopFileWatchers();
            StopCssFileWatchers();

            _debounce.Dispose();
            _cssDebounce.Dispose();
            _lateLoadDebounce.Dispose();
        }
    }

    private void StartFileWatchers()
    {
        lock (_watcherLock)
        {
            if (_disposed)
            {
                return;
            }

            StartFileWatchersCore();
        }
    }

    private void StartFileWatchersCore()
    {
        foreach (var dir in _options.WatchSourceDirectories)
        {
            if (!Directory.Exists(dir))
            {
                LogMonorailcssDiscoveryWatchsourcedirectoriesEntryDirDoesNotExist(dir);
                continue;
            }

            var watcher = new FileSystemWatcher(dir)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,

                // These watch whole project trees (incl. build-output subtrees a rebuild churns).
                // The filename filters gate what reaches our handlers, but the OS still buffers the
                // raw notification stream first, so widen the buffer to make overflow less likely.
                InternalBufferSize = 64 * 1024,
                EnableRaisingEvents = true,
            };

            watcher.Filters.Add("*.razor");
            watcher.Filters.Add("*.cshtml");
            watcher.Filters.Add("*.cs");
            watcher.Filters.Add("*.html");

            // Also watch the static-web-asset script types (default .js/.mjs). Startup already
            // scans these through the asset manifest, but the manifest is read once, so a live edit
            // to a component script that builds markup (class strings inside .js) would otherwise be
            // invisible until the next process start. Gated on the same toggle + extension set that
            // govern static-web-asset *scanning* so the watch signal and the scan can't diverge.
            if (_options.ScanStaticWebAssets)
            {
                foreach (var ext in _options.StaticWebAssetExtensions)
                {
                    watcher.Filters.Add("*" + ext);
                }
            }

            watcher.Changed += OnSourceFileChanged;
            watcher.Created += OnSourceFileChanged;
            watcher.Renamed += OnSourceFileRenamed;
            watcher.Error += OnSourceWatcherError;

            _fileWatchers.Add(watcher);
            LogMonorailcssDiscoveryWatchingSourceDirectoryDir(dir);
        }
    }

    private void StopFileWatchers()
    {
        lock (_watcherLock)
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
    }

    /// <summary>
    /// Watches every file the source-CSS pipeline pulled in (entry path + every transitively
    /// imported file). Edits to any of them trigger a regeneration. We use a per-directory
    /// <see cref="FileSystemWatcher"/> with a filename filter so cross-directory imports
    /// (e.g. NuGet-shipped theme files under <c>_content/</c>) work too.
    /// </summary>
    private void StartCssFileWatchers()
    {
        lock (_watcherLock)
        {
            if (_disposed)
            {
                return;
            }

            StartCssFileWatchersCore();
        }
    }

    private void StartCssFileWatchersCore()
    {
        if (_result.ImportedCssFiles.Count == 0)
        {
            return;
        }

        if (_environment != null && !_environment.IsDevelopment())
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
            LogMonorailcssDiscoveryWatchingCssInDirCountFiles(dir, group.Count());
        }
    }

    private void StopCssFileWatchers()
    {
        lock (_watcherLock)
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
        LogMonorailcssDiscoverySourceCssChangedReProcessing();
        Regenerate("css-watcher");

        // Imported file set may have shifted (a new @import landed, a removed one disappeared).
        StopCssFileWatchers();
        StartCssFileWatchers();
    }

    private void OnSourceFileChanged(object sender, FileSystemEventArgs e) => QueueSourceFile(e.FullPath);

    private void OnSourceFileRenamed(object sender, RenamedEventArgs e) => QueueSourceFile(e.FullPath);

    private void OnSourceWatcherError(object sender, ErrorEventArgs e)
    {
        // The OS dropped a notification window — typically internal-buffer overflow during a build
        // burst across the watched project trees. Edits in that window are now invisible to the
        // incremental path, so schedule a full rescan to re-seed from disk instead of waiting for
        // an unrelated trigger to incidentally pick them up.
        LogMonorailcssDiscoverySourceWatcherErrorSchedulingFullRescan(e.GetException());

        lock (_pendingFiles)
        {
            _pendingFullRescan = true;
        }

        _debounce.Change(150, Timeout.Infinite);
    }

    private void QueueSourceFile(string path)
    {
        if (DiscoveryPaths.IsInIgnoredDirectory(path))
        {
            return;
        }

        lock (_pendingFiles)
        {
            _pendingFiles.Add(path);
        }

        _debounce.Change(150, Timeout.Infinite);
    }

    private void OnDebounceTick(object? state)
    {
        string[] paths;
        bool fullRescan;
        lock (_pendingFiles)
        {
            paths = _pendingFiles.ToArray();
            _pendingFiles.Clear();
            fullRescan = _pendingFullRescan;
            _pendingFullRescan = false;
        }

        if (fullRescan)
        {
            // A dropped-event window supersedes the incremental list: re-walk every watched
            // directory so nothing edited during the gap is missed.
            Regenerate("watcher-error");
            return;
        }

        if (paths.Length == 0)
        {
            return;
        }

        LogMonorailcssDiscoverySourceFileChangeCountFiles(paths.Length, string.Join(", ", paths.Select(Path.GetFileName)));

        Regenerate("source-watcher", paths);
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

        LogMonorailcssDiscoveryHotReloadEventCountChangedCountChangedAssembliesNames(_hotReloadCount, changedList.Length, string.Join(", ", changedList.Select(a => a.GetName().Name)));

        Regenerate("hot-reload");
    }

    private void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
    {
        if (!_started)
        {
            return;
        }

        // ASP.NET / Blazor JIT-loads many referenced assemblies in a burst the first time a page
        // is served, but the loads can be spread out over hundreds of ms each (one per component
        // resolved). 750ms is wide enough to absorb the typical burst yet still feels responsive
        // when an assembly is genuinely loaded out-of-band later.
        _lateLoadDebounce.Change(750, Timeout.Infinite);
    }

    private void OnLateLoadDebounceTick(object? state)
    {
        Regenerate("late-load");
    }

    private void Regenerate(string trigger, IReadOnlyCollection<string>? changedSourceFiles = null)
    {
        var sw = Stopwatch.StartNew();
        var previousEtag = _result.ETag;

        // Late-load and hot-reload events don't touch source files; signaling
        // SkipSourceFileScan lets the generator skip the directory walk and per-file mtime
        // checks and replay the previous source-file token contribution. Source-watcher
        // passes the exact set of changed files so the generator can rescan just those
        // (Tailwind-style incremental). css-watcher and startup need a fresh full scan
        // because either the source CSS just changed (and its @source/@safelist may have
        // shifted) or we have no prior state to incrementally update.
        var skipSourceFileScan = trigger is "late-load" or "hot-reload";
        var hasChangedFiles = changedSourceFiles is { Count: > 0 };

        var result = _generator.Generate(new MonorailCssGenerationRequest
        {
            SourceCss = _options.SourceCss,
            SourceCssPath = _options.SourceCssPath,
            BaseFramework = _options.Framework,
            Assemblies = AppDomain.CurrentDomain.GetAssemblies(),
            SourceFiles = (skipSourceFileScan || hasChangedFiles) ? Array.Empty<string>() : EnumerateScanSourceFiles(),
            ExtraSafelist = _options.ExtraSafelist,
            ExcludeAssemblies = _options.ExcludeAssemblies,
            SkipSourceFileScan = skipSourceFileScan,
            ChangedSourceFiles = changedSourceFiles,
        });
        sw.Stop();

        var unchanged = string.Equals(result.ETag, previousEtag, StringComparison.Ordinal);

        lock (_lock)
        {
            _result = result;
            _lastRegeneratedAt = DateTime.UtcNow;
            _regenerateCount++;

            // Surface the generator-built framework so options consumers can read it back.
            _options.Framework = result.Framework;
        }

        if (unchanged)
        {
            LogMonorailcssDiscoveryNoOpRegenTriggerTriggerEtagEtagInElapsedMs(trigger, result.ETag, sw.ElapsedMilliseconds);
            return;
        }

        LogMonorailcssDiscoveryRegeneratedCssTriggerClassesCountLength(trigger, result.Classes.Count, result.Css.Length, result.ETag, sw.ElapsedMilliseconds);

        if (!string.IsNullOrEmpty(_options.WriteToFile))
        {
            TryWriteToFile(result.Css);
        }
    }

    private List<string> EnumerateScanSourceFiles()
    {
        var files = EnumerateWatchedSourceFiles();
        files.AddRange(GetStaticWebAssetFiles());
        return files;
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

            EnumerateSourceFilesInto(dir, files);
        }

        return files;
    }

    /// <summary>
    /// Resolves the physical paths of package-shipped static web assets (e.g.
    /// <c>_content/Pennington.UI/scripts.js</c>) eligible for scanning. The result is computed
    /// once and cached for the process: the entry assembly's runtime manifest is fixed at
    /// build time, and the resolved files (NuGet-cache paths) are immutable, so re-reading the
    /// manifest on every regeneration would be wasted I/O. The files flow through the same
    /// <c>SourceFileScanner</c> as ordinary source files, so they inherit its per-file mtime
    /// and incremental token caches.
    /// </summary>
    private IReadOnlyList<string> GetStaticWebAssetFiles()
    {
        return _staticWebAssetFiles ??= ResolveStaticWebAssetFiles();
    }

    private List<string> ResolveStaticWebAssetFiles()
    {
        var result = new List<string>();
        if (!_options.ScanStaticWebAssets)
        {
            return result;
        }

        var entry = Assembly.GetEntryAssembly();
        if (entry is null)
        {
            return result;
        }

        var manifestPath = StaticWebAssetManifest.GetManifestPath(entry);
        if (manifestPath is null || !File.Exists(manifestPath))
        {
            return result;
        }

        string json;
        try
        {
            json = File.ReadAllText(manifestPath);
        }
        catch (IOException)
        {
            return result;
        }
        catch (UnauthorizedAccessException)
        {
            return result;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var asset in StaticWebAssetManifest.Resolve(json))
        {
            var ext = Path.GetExtension(asset.PhysicalPath);
            if (!_options.StaticWebAssetExtensions.Contains(ext))
            {
                continue;
            }

            if (IsExcludedStaticWebAssetPackage(asset.UrlPath))
            {
                continue;
            }

            if (seen.Add(asset.PhysicalPath) && File.Exists(asset.PhysicalPath))
            {
                result.Add(asset.PhysicalPath);
            }
        }

        LogMonorailcssDiscoveryStaticWebAssetsScannedCountManifest(result.Count, manifestPath);
        return result;
    }

    /// <summary>
    /// Maps a served URL path back to its owning package and tests it against
    /// <see cref="MonorailDiscoveryOptions.ExcludeAssemblies"/>. Served paths look like
    /// <c>_content/&lt;Package&gt;/path/to.js</c>, and an RCL's <c>_content</c> base segment is
    /// its assembly name, so excluding the assembly also suppresses its shipped assets. Assets
    /// outside <c>_content</c> (the app's own) are never excluded.
    /// </summary>
    private bool IsExcludedStaticWebAssetPackage(string urlPath)
    {
        const string prefix = "_content/";
        if (!urlPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var rest = urlPath.AsSpan(prefix.Length);
        var slash = rest.IndexOf('/');
        if (slash <= 0)
        {
            return false;
        }

        var package = rest.Slice(0, slash).ToString();
        return _options.ExcludeAssemblies.Contains(package);
    }

    private static void EnumerateSourceFilesInto(string root, List<string> output)
    {
        // Iterative DFS so we can prune subtrees by directory name before recursing into them.
        // Using TopDirectoryOnly + a stack is dramatically cheaper than
        // SearchOption.AllDirectories + a post-filter when the tree contains node_modules /
        // .git / bin / obj — those subtrees can dwarf the rest of the project.
        var stack = new Stack<string>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var dir = stack.Pop();

            // Collect files first so a malformed subdirectory enumeration doesn't lose them.
            IEnumerable<string>? files = null;
            try
            {
                files = Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                // best-effort
            }

            if (files is not null)
            {
                foreach (var path in files)
                {
                    if (DiscoveryPaths.HasSupportedExtension(path))
                    {
                        output.Add(path);
                    }
                }
            }

            IEnumerable<string>? subdirs = null;
            try
            {
                subdirs = Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                // best-effort
            }

            if (subdirs is not null)
            {
                foreach (var sub in subdirs)
                {
                    var name = Path.GetFileName(sub);
                    if (!string.IsNullOrEmpty(name) && !DiscoveryPaths.IgnoredDirectoryNames.Contains(name))
                    {
                        stack.Push(sub);
                    }
                }
            }
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

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: auto-watching {Dir}")]
    partial void LogMonorailcssDiscoveryAutoWatchingDir(string dir);

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: auto-detected source CSS at {Path}")]
    partial void LogMonorailcssDiscoveryAutoDetectedSourceCssAtPath(string path);

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: hot-reload event #{Count} — {ChangedCount} changed assemblies: {Names}")]
    partial void LogMonorailcssDiscoveryHotReloadEventCountChangedCountChangedAssembliesNames(int count, int changedCount, string names);

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: no-op regen (trigger={Trigger}, etag={Etag}, in {ElapsedMs} ms)")]
    partial void LogMonorailcssDiscoveryNoOpRegenTriggerTriggerEtagEtagInElapsedMs(string trigger, string etag, long elapsedMs);

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: regenerated CSS (trigger={Trigger}, classes={ClassCount}, length={Length}, etag={Etag}, in {ElapsedMs} ms)")]
    partial void LogMonorailcssDiscoveryRegeneratedCssTriggerClassesCountLength(string trigger, int classCount, int length, string etag, long elapsedMs);

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: source file change ({Count} files): {Files}")]
    partial void LogMonorailcssDiscoverySourceFileChangeCountFiles(int count, string files);

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: source CSS changed — re-processing")]
    partial void LogMonorailcssDiscoverySourceCssChangedReProcessing();

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: watching CSS in {Dir} ({Count} files)")]
    partial void LogMonorailcssDiscoveryWatchingCssInDirCountFiles(string dir, int count);

    [LoggerMessage(LogLevel.Warning, "MonorailCss discovery: WatchSourceDirectories entry {Dir} does not exist")]
    partial void LogMonorailcssDiscoveryWatchsourcedirectoriesEntryDirDoesNotExist(string dir);

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: watching source directory {Dir}")]
    partial void LogMonorailcssDiscoveryWatchingSourceDirectoryDir(string dir);

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: watch-mode discovered {RootCount} referenced project source root(s), added {AddedCount} new")]
    partial void LogMonorailcssDiscoveryReferencedProjectRootsDiscovered(int rootCount, int addedCount);

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: watching referenced project source {Dir}")]
    partial void LogMonorailcssDiscoveryWatchingReferencedProjectSource(string dir);

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: source watcher reported an error — scheduling a full rescan")]
    partial void LogMonorailcssDiscoverySourceWatcherErrorSchedulingFullRescan(Exception exception);

    [LoggerMessage(LogLevel.Trace, "MonorailCss discovery: skipped reference {Name} (load failed)")]
    partial void LogMonorailcssDiscoverySkippedReferenceNameLoadFailed(string name, Exception exception);

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery startup: {ClassCount} classes in {ElapsedMs} ms — ETag {Etag}")]
    partial void LogMonorailcssDiscoveryStartupClassCloutElapsedMsEtag(int classCount, long elapsedMs, string etag);

    [LoggerMessage(LogLevel.Debug, "MonorailCss discovery: scanned {Count} static web asset file(s) from {ManifestPath}")]
    partial void LogMonorailcssDiscoveryStaticWebAssetsScannedCountManifest(int count, string manifestPath);
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
    IReadOnlyList<string> WatchSourceDirectories,
    IReadOnlyList<string> AllClasses);
