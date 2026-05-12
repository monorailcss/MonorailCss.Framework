using MonorailCss.Build.Tasks.BuildCache;
using MonorailCss.Discovery;
using Shouldly;

namespace MonorailCss.Build.Tasks.Tests;

/// <summary>
/// Round-trip + corruption-recovery coverage for the persistent JSON cache that
/// <see cref="MonorailCss.Build.Tasks.ProcessCssTask"/> uses to carry per-source-file and
/// per-MVID assembly scan state across <c>dotnet build</c> invocations under
/// <c>dotnet watch</c>.
/// </summary>
public class PersistentGenerationCacheTests
{
    [Fact]
    public void TryLoad_On_Missing_File_Returns_Null()
    {
        using var ws = new TestWorkspace();
        var cache = new PersistentGenerationCache(ws.Root);

        cache.TryLoad().ShouldBeNull();
    }

    [Fact]
    public void Save_Then_TryLoad_Round_Trips_Source_Files()
    {
        using var ws = new TestWorkspace();
        var cache = new PersistentGenerationCache(ws.Root);

        var mtime = new DateTime(2026, 5, 1, 12, 30, 0, DateTimeKind.Utc);
        var snapshot = new GenerationCacheSnapshot(
            SourceFiles: new[]
            {
                new SourceFileCacheEntry("C:/x/a.razor", mtime, new[] { "bg-red-500", "p-4" }),
                new SourceFileCacheEntry("C:/x/b.razor", mtime.AddMinutes(1), new[] { "underline" }),
            },
            Assemblies: Array.Empty<AssemblyCacheEntry>());

        cache.Save(snapshot);

        var loaded = cache.TryLoad();
        loaded.ShouldNotBeNull();
        loaded.SourceFiles.Count.ShouldBe(2);
        loaded.SourceFiles[0].Path.ShouldBe("C:/x/a.razor");
        loaded.SourceFiles[0].Mtime.ShouldBe(mtime);
        loaded.SourceFiles[0].Tokens.ShouldBe(new[] { "bg-red-500", "p-4" });
        loaded.SourceFiles[1].Path.ShouldBe("C:/x/b.razor");
        loaded.SourceFiles[1].Tokens.ShouldBe(new[] { "underline" });
    }

    [Fact]
    public void Save_Then_TryLoad_Round_Trips_Assembly_Entries()
    {
        using var ws = new TestWorkspace();
        var cache = new PersistentGenerationCache(ws.Root);

        var mvid = Guid.NewGuid();
        var mtime = DateTime.UtcNow.AddDays(-1);
        var snapshot = new GenerationCacheSnapshot(
            SourceFiles: Array.Empty<SourceFileCacheEntry>(),
            Assemblies: new[]
            {
                new AssemblyCacheEntry("C:/bin/Foo.dll", mtime, mvid, new[] { "bg-red-500" }),
            });

        cache.Save(snapshot);

        var loaded = cache.TryLoad();
        loaded.ShouldNotBeNull();
        loaded.Assemblies.Count.ShouldBe(1);
        loaded.Assemblies[0].Path.ShouldBe("C:/bin/Foo.dll");
        loaded.Assemblies[0].Mvid.ShouldBe(mvid);
        loaded.Assemblies[0].Mtime.ShouldBe(mtime);
        loaded.Assemblies[0].Tokens.ShouldBe(new[] { "bg-red-500" });
    }

    [Fact]
    public void TryLoad_On_Corrupt_File_Returns_Null()
    {
        // A partially-written cache from a killed dotnet-watch must never fail the build —
        // worst case is a cold rebuild.
        using var ws = new TestWorkspace();
        var cache = new PersistentGenerationCache(ws.Root);
        File.WriteAllText(cache.CacheFilePath, "{\"version\": 1, \"sourceFiles\": [{\"path\"");

        cache.TryLoad().ShouldBeNull();
    }

    [Fact]
    public void TryLoad_On_Unknown_Version_Returns_Null()
    {
        // Forward-compatibility guard: a newer SDK that wrote a v99 cache should not crash
        // the older task — it just rescans.
        using var ws = new TestWorkspace();
        var cache = new PersistentGenerationCache(ws.Root);
        File.WriteAllText(cache.CacheFilePath, "{\"version\": 99}");

        cache.TryLoad().ShouldBeNull();
    }

    [Fact]
    public void Save_Atomically_Replaces_Existing_File()
    {
        // Two consecutive saves replace cleanly; no stray .tmp file should remain.
        using var ws = new TestWorkspace();
        var cache = new PersistentGenerationCache(ws.Root);

        cache.Save(new GenerationCacheSnapshot(
            SourceFiles: new[] { new SourceFileCacheEntry("p1", DateTime.UtcNow, new[] { "tok-a" }) },
            Assemblies: Array.Empty<AssemblyCacheEntry>()));

        cache.Save(new GenerationCacheSnapshot(
            SourceFiles: new[] { new SourceFileCacheEntry("p2", DateTime.UtcNow, new[] { "tok-b" }) },
            Assemblies: Array.Empty<AssemblyCacheEntry>()));

        File.Exists(cache.CacheFilePath + ".tmp").ShouldBeFalse();
        var loaded = cache.TryLoad();
        loaded.ShouldNotBeNull();
        loaded.SourceFiles.Single().Path.ShouldBe("p2");
    }
}
