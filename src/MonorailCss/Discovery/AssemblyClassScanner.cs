using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace MonorailCss.Discovery;

/// <summary>
/// Scans the <c>#US</c> (User Strings) heap of an assembly's IL metadata, extracting every
/// string literal and validating each one against the framework's candidate parser.
/// <para>
/// Two entry points share a single MVID-keyed cache:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Scan(Assembly, ICollection{string})"/> reads in-memory
///     metadata via <c>System.Reflection.Metadata.AssemblyExtensions.TryGetRawMetadata</c>,
///     which exposes the live image including hot-reload deltas. Used by the runtime
///     discovery pipeline.</description></item>
///   <item><description><see cref="ScanFile"/> opens a PE file from disk and reads its
///     metadata via <c>PEReader</c>. Used by the build-time MSBuild task. An outer
///     path+mtime cache lets a warm build skip even the file open if the DLL on disk hasn't
///     changed since the previous task invocation.</description></item>
/// </list>
/// <para>
/// The actual heap walk and the skip checks (reference assembly, <c>[assembly: MonorailCssNoScan]</c>)
/// live in <see cref="IlMetadataScanner"/> so both entry points share one implementation.
/// </para>
/// </summary>
internal sealed class AssemblyClassScanner
{
    // MVID is stable for the lifetime of a loaded module — even hot-reload deltas leave the base
    // PE image (and its MVID) unchanged. Caching by MVID lets a hot-reload event short-circuit
    // 16 of 17 assemblies on a typical docs site, since their #US heaps are byte-identical to
    // the previous scan. Hit count is exposed via <see cref="MvidCacheHits"/>.
    private readonly ConcurrentDictionary<Guid, ImmutableSortedSet<string>> _mvidCache = new();

    // Path+mtime outer cache: lets the build-task path skip opening the PE file when the DLL on
    // disk hasn't been touched since the previous task invocation. Seeded from the persistent
    // cache at task start so the first ScanFile call after a process restart still hits.
    private readonly ConcurrentDictionary<string, PathEntry> _pathCache =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly ValidationCache _validationCache;
    private long _mvidCacheHits;
    private long _mvidCacheMisses;

    public AssemblyClassScanner(ValidationCache validationCache)
    {
        _validationCache = validationCache;
    }

    public long MvidCacheHits => Interlocked.Read(ref _mvidCacheHits);

    public long MvidCacheMisses => Interlocked.Read(ref _mvidCacheMisses);

    /// <summary>
    /// Scans <paramref name="assembly"/> and adds every valid candidate string to <paramref name="output"/>.
    /// Returns false if the assembly was skipped (no metadata or reference-only).
    /// </summary>
    public bool Scan(Assembly assembly, ICollection<string> output)
    {
        return ScanCore(assembly, output);
    }

    private unsafe bool ScanCore(Assembly assembly, ICollection<string> output)
    {
        if (!assembly.TryGetRawMetadata(out var blob, out var length))
        {
            return false;
        }

        var md = new MetadataReader(blob, length);

        if (!md.IsAssembly)
        {
            return false;
        }

        if (IlMetadataScanner.HasReferenceAssemblyAttribute(md))
        {
            return false;
        }

        if (IlMetadataScanner.HasMonorailCssNoScanAttribute(md))
        {
            return false;
        }

        var mvid = md.GetGuid(md.GetModuleDefinition().Mvid);

        // Hot-reload events ask us to rescan every loaded assembly even though usually only
        // one delta-modified assembly's content is novel. MVID is stable across the lifetime
        // of the module (deltas don't change it), so a cache keyed on MVID returns the
        // previously-validated set instantly for unchanged assemblies.
        if (_mvidCache.TryGetValue(mvid, out var cached))
        {
            Interlocked.Increment(ref _mvidCacheHits);
            foreach (var c in cached)
            {
                output.Add(c);
            }

            return true;
        }

        Interlocked.Increment(ref _mvidCacheMisses);

        // Buffer into a local set so we can both populate the caller's collection and cache
        // the dedup'd, sorted result for the next rescan.
        var local = new HashSet<string>(StringComparer.Ordinal);
        IlMetadataScanner.ScanUserStringHeap(md, _validationCache, local);

        var snapshot = local.ToImmutableSortedSet(StringComparer.Ordinal);
        _mvidCache[mvid] = snapshot;
        foreach (var c in snapshot)
        {
            output.Add(c);
        }

        return true;
    }

    /// <summary>
    /// Scans <paramref name="path"/> (an on-disk DLL) and adds every valid candidate string to
    /// <paramref name="output"/>. Returns false when the file is unreadable, has no metadata,
    /// or is a reference assembly.
    /// <para>
    /// Three cache layers:
    /// </para>
    /// <list type="number">
    ///   <item><description>If the file's current mtime matches the path+mtime cache entry
    ///     (typically seeded from a previous build), replay the cached MVID's tokens without
    ///     opening the file.</description></item>
    ///   <item><description>Otherwise open the file, read MVID, and replay the MVID cache if
    ///     present (a rebuilt-but-identical DLL still hits here).</description></item>
    ///   <item><description>Otherwise run the full <c>#US</c> heap scan and populate both
    ///     caches.</description></item>
    /// </list>
    /// </summary>
    public bool ScanFile(string path, ICollection<string> output)
    {
        DateTime currentMtime;
        try
        {
            currentMtime = File.GetLastWriteTimeUtc(path);
        }
        catch
        {
            return false;
        }

        // Layer 1: path+mtime cache hit. No file open, no metadata read.
        if (_pathCache.TryGetValue(path, out var pathEntry) && pathEntry.Mtime == currentMtime
            && _mvidCache.TryGetValue(pathEntry.Mvid, out var pathCached))
        {
            Interlocked.Increment(ref _mvidCacheHits);
            foreach (var c in pathCached)
            {
                output.Add(c);
            }

            return true;
        }

        Guid mvid;
        ImmutableSortedSet<string>? mvidCached;
        try
        {
            using var stream = File.OpenRead(path);
            using var peReader = new PEReader(stream);
            if (!peReader.HasMetadata)
            {
                return false;
            }

            var reader = peReader.GetMetadataReader();
            if (!reader.IsAssembly
                || IlMetadataScanner.HasReferenceAssemblyAttribute(reader)
                || IlMetadataScanner.HasMonorailCssNoScanAttribute(reader))
            {
                return false;
            }

            mvid = reader.GetGuid(reader.GetModuleDefinition().Mvid);

            // Layer 2: MVID cache hit. Metadata read but no IL scan needed.
            if (_mvidCache.TryGetValue(mvid, out mvidCached))
            {
                Interlocked.Increment(ref _mvidCacheHits);
                _pathCache[path] = new PathEntry(currentMtime, mvid);
                foreach (var c in mvidCached)
                {
                    output.Add(c);
                }

                return true;
            }

            // Layer 3: full scan.
            Interlocked.Increment(ref _mvidCacheMisses);
            var local = new HashSet<string>(StringComparer.Ordinal);
            IlMetadataScanner.ScanUserStringHeap(reader, _validationCache, local);
            var snapshot = local.ToImmutableSortedSet(StringComparer.Ordinal);
            _mvidCache[mvid] = snapshot;
            _pathCache[path] = new PathEntry(currentMtime, mvid);
            foreach (var c in snapshot)
            {
                output.Add(c);
            }

            return true;
        }
        catch
        {
            // Best-effort: a malformed DLL or a transient read error shouldn't fail the
            // whole generation. The candidate set is still useful from the rest.
            return false;
        }
    }

    /// <summary>
    /// Imports cached per-MVID token sets and path+mtime entries from a prior process,
    /// typically read from <c>obj/MonorailCss/assembly-tokens.json</c>. Subsequent
    /// <see cref="ScanFile"/> calls will hit the cache without opening or re-scanning the
    /// underlying DLL. Existing entries with the same MVID are left untouched.
    /// </summary>
    public void SeedCache(IEnumerable<AssemblyCacheEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (!_mvidCache.ContainsKey(entry.Mvid))
            {
                var tokens = entry.Tokens is ImmutableSortedSet<string> sorted
                    ? sorted
                    : entry.Tokens.ToImmutableSortedSet(StringComparer.Ordinal);
                _mvidCache[entry.Mvid] = tokens;
            }

            if (!string.IsNullOrEmpty(entry.Path))
            {
                _pathCache[entry.Path] = new PathEntry(entry.Mtime, entry.Mvid);
            }
        }
    }

    /// <summary>
    /// Snapshots the path+mtime cache joined to the MVID cache for persistence. Only entries
    /// in <see cref="_pathCache"/> appear — MVID-only entries (from runtime
    /// <see cref="Scan(Assembly, ICollection{string})"/>) are not serialized because they have
    /// no path to re-correlate on the next process. Caller may filter the result to the set
    /// of paths it still considers relevant (deleted DLLs drop out naturally that way).
    /// </summary>
    public IReadOnlyList<AssemblyCacheEntry> SnapshotCache()
    {
        var result = new List<AssemblyCacheEntry>(_pathCache.Count);
        foreach (var kvp in _pathCache)
        {
            if (_mvidCache.TryGetValue(kvp.Value.Mvid, out var tokens))
            {
                result.Add(new AssemblyCacheEntry(kvp.Key, kvp.Value.Mtime, kvp.Value.Mvid, tokens));
            }
        }

        return result;
    }

    private readonly record struct PathEntry(DateTime Mtime, Guid Mvid);
}

/// <summary>
/// One entry in the persistent assembly token cache: a DLL path, its last-known mtime, its
/// module MVID, and the validated candidate strings extracted from its <c>#US</c> heap.
/// </summary>
public sealed record AssemblyCacheEntry(string Path, DateTime Mtime, Guid Mvid, IReadOnlyCollection<string> Tokens);
