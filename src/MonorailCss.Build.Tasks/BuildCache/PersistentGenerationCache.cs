using System.Text.Json;
using System.Text.Json.Serialization;
using MonorailCss.Discovery;

namespace MonorailCss.Build.Tasks.BuildCache;

/// <summary>
/// Disk-backed persistence for <see cref="MonorailCssGenerator"/>'s per-MVID assembly token
/// cache and per-source-file token cache. Lives under
/// <c>$(IntermediateOutputPath)MonorailCss/</c> so it's cleaned by <c>dotnet clean</c> and
/// follows the project's intermediate output convention.
/// <para>
/// Without this, each <c>dotnet build</c> invocation (including those triggered by
/// <c>dotnet watch</c>) starts the generator with empty caches and re-scans every assembly +
/// source file from scratch. With it, an unchanged DLL is replayed from cache, an unchanged
/// source file is replayed by mtime, and only the file the user just touched is re-tokenized.
/// </para>
/// </summary>
internal sealed class PersistentGenerationCache
{
    // Bump when the on-disk schema changes in a backward-incompatible way. Older versions are
    // ignored gracefully (treated as a cold start) rather than throwing.
    private const int CurrentVersion = 1;
    private const string CacheFileName = "cache.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string _cacheFilePath;

    public PersistentGenerationCache(string cacheDirectory)
    {
        _cacheFilePath = Path.Combine(cacheDirectory, CacheFileName);
    }

    public string CacheFilePath => _cacheFilePath;

    /// <summary>
    /// Reads the cache from disk and returns it as a <see cref="GenerationCacheSeed"/> the
    /// generator can ingest. Returns null on a missing, unreadable, or version-mismatched
    /// file — every error path is treated as a cold start because a corrupt cache is never
    /// worth failing the build over.
    /// </summary>
    public GenerationCacheSeed? TryLoad()
    {
        if (!File.Exists(_cacheFilePath))
        {
            return null;
        }

        try
        {
            using var stream = File.OpenRead(_cacheFilePath);
            var dto = JsonSerializer.Deserialize<CacheDto>(stream, SerializerOptions);
            if (dto is null || dto.Version != CurrentVersion)
            {
                return null;
            }

            IReadOnlyList<SourceFileCacheEntry> sourceFiles = dto.SourceFiles is null
                ? Array.Empty<SourceFileCacheEntry>()
                : dto.SourceFiles
                    .Where(e => !string.IsNullOrEmpty(e.Path))
                    .Select(e => new SourceFileCacheEntry(e.Path, new DateTime(e.MtimeTicks, DateTimeKind.Utc), e.Tokens ?? []))
                    .ToList();

            IReadOnlyList<AssemblyCacheEntry> assemblies = dto.Assemblies is null
                ? Array.Empty<AssemblyCacheEntry>()
                : dto.Assemblies
                    .Where(e => !string.IsNullOrEmpty(e.Path))
                    .Select(e => new AssemblyCacheEntry(e.Path, new DateTime(e.MtimeTicks, DateTimeKind.Utc), e.Mvid, e.Tokens ?? []))
                    .ToList();

            return new GenerationCacheSeed(sourceFiles, assemblies);
        }
        catch
        {
            // Corrupt or partially-written file — cold start.
            return null;
        }
    }

    /// <summary>
    /// Writes <paramref name="snapshot"/> to disk atomically: serialize to a sibling
    /// <c>.tmp</c> file, then <see cref="File.Move(string, string, bool)"/> over the real
    /// path. A killed <c>dotnet watch</c> mid-write can leave a leftover <c>.tmp</c> but
    /// never a half-written <c>cache.json</c>.
    /// </summary>
    public void Save(GenerationCacheSnapshot snapshot)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_cacheFilePath)!);

        var dto = new CacheDto
        {
            Version = CurrentVersion,
            SourceFiles = snapshot.SourceFiles
                .Select(e => new SourceFileDto
                {
                    Path = e.Path,
                    MtimeTicks = e.Mtime.Ticks,
                    Tokens = e.Tokens.ToArray(),
                })
                .ToArray(),
            Assemblies = snapshot.Assemblies
                .Select(e => new AssemblyDto
                {
                    Path = e.Path,
                    MtimeTicks = e.Mtime.Ticks,
                    Mvid = e.Mvid,
                    Tokens = e.Tokens.ToArray(),
                })
                .ToArray(),
        };

        var tempPath = _cacheFilePath + ".tmp";
        try
        {
            using (var stream = File.Create(tempPath))
            {
                JsonSerializer.Serialize(stream, dto, SerializerOptions);
            }

            File.Move(tempPath, _cacheFilePath, overwrite: true);
        }
        catch
        {
            // Best-effort persistence: a write failure (full disk, locked file, etc.) is
            // never worth failing the build over — the next run just falls back to a cold
            // scan. Clean up the partial temp if we can.
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch
            {
                // Nothing more we can do.
            }
        }
    }

    private sealed class CacheDto
    {
        public int Version { get; set; }

        public SourceFileDto[]? SourceFiles { get; set; }

        public AssemblyDto[]? Assemblies { get; set; }
    }

    private sealed class SourceFileDto
    {
        public string Path { get; set; } = string.Empty;

        public long MtimeTicks { get; set; }

        public string[]? Tokens { get; set; }
    }

    private sealed class AssemblyDto
    {
        public string Path { get; set; } = string.Empty;

        public long MtimeTicks { get; set; }

        public Guid Mvid { get; set; }

        public string[]? Tokens { get; set; }
    }
}
