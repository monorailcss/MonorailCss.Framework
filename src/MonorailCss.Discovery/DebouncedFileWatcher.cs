namespace MonorailCss.Discovery;

/// <summary>
/// Bundles a set of <see cref="FileSystemWatcher"/> instances behind a single debounced trigger.
/// Editor saves and tooling-driven rewrites typically fire multiple change events in quick
/// succession; the watcher coalesces them into one <c>onTick</c> invocation per debounce window.
/// Used twice in <see cref="ClassDiscoveryService"/> — once for the recursive source-directory
/// watch, once for the per-directory CSS-import watch.
/// </summary>
internal sealed class DebouncedFileWatcher : IDisposable
{
    private readonly List<FileSystemWatcher> _watchers = new();
    private readonly System.Threading.Timer _debounce;
    private readonly Action _onTick;
    private readonly int _debounceMs;

    public DebouncedFileWatcher(Action onTick, int debounceMs = 150)
    {
        _onTick = onTick;
        _debounceMs = debounceMs;
        _debounce = new System.Threading.Timer(_ => _onTick(), null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Registers a watcher for <paramref name="directory"/>. Returns false if the directory
    /// does not exist or is already covered by a previously-added watcher (matched by
    /// case-insensitive normalized full path). When <paramref name="onChange"/> is supplied it
    /// is invoked with the changed file's full path before the debounce trigger fires.
    /// </summary>
    public bool AddDirectory(
        string directory,
        IReadOnlyCollection<string> filters,
        bool includeSubdirectories,
        Action<string>? onChange = null)
    {
        if (!Directory.Exists(directory))
        {
            return false;
        }

        var normalized = NormalizeDirectory(directory);
        foreach (var existing in _watchers)
        {
            if (string.Equals(NormalizeDirectory(existing.Path), normalized, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        var watcher = new FileSystemWatcher(directory)
        {
            IncludeSubdirectories = includeSubdirectories,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
            InternalBufferSize = 64 * 1024,
            EnableRaisingEvents = true,
        };

        foreach (var filter in filters)
        {
            watcher.Filters.Add(filter);
        }

        // Recursive watches against a project root pick up obj/, bin/, and .vs/, where
        // Razor SDK output, EnC delta staging, and source generators churn *.cs files
        // constantly. Forwarding those events races dotnet watch's hot reload (CS7038).
        void Handle(string fullPath)
        {
            if (IsBuildIntermediate(fullPath))
            {
                return;
            }

            onChange?.Invoke(fullPath);
            Trigger();
        }

        watcher.Changed += (_, e) => Handle(e.FullPath);
        watcher.Created += (_, e) => Handle(e.FullPath);
        watcher.Renamed += (_, e) => Handle(e.FullPath);

        _watchers.Add(watcher);
        return true;
    }

    /// <summary>
    /// Schedules <c>onTick</c> to fire after the debounce window. Subsequent calls within the
    /// window reset the timer so the callback runs once for any burst.
    /// </summary>
    public void Trigger() => _debounce.Change(_debounceMs, Timeout.Infinite);

    /// <summary>
    /// Disposes every registered watcher and clears the list. Leaves the debounce timer alive
    /// so the same instance can be reused after a subsequent <see cref="AddDirectory"/> call.
    /// </summary>
    public void Stop()
    {
        foreach (var w in _watchers)
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

        _watchers.Clear();
    }

    public void Dispose()
    {
        Stop();
        _debounce.Dispose();
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

    private static bool IsBuildIntermediate(string fullPath)
    {
        var sep = Path.DirectorySeparatorChar;
        return fullPath.Contains($"{sep}obj{sep}", StringComparison.OrdinalIgnoreCase)
            || fullPath.Contains($"{sep}bin{sep}", StringComparison.OrdinalIgnoreCase)
            || fullPath.Contains($"{sep}.vs{sep}", StringComparison.OrdinalIgnoreCase);
    }
}
