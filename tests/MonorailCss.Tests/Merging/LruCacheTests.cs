using MonorailCss.Merging;
using Shouldly;

namespace MonorailCss.Tests.Merging;

public class LruCacheTests
{
    [Fact]
    public void Set_EvictsLeastRecentlyUsedAtCapacity()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.Set("c", 3);

        cache.TryGetValue("a", out _).ShouldBeFalse();
        cache.TryGetValue("b", out var b).ShouldBeTrue();
        b.ShouldBe(2);
        cache.TryGetValue("c", out var c).ShouldBeTrue();
        c.ShouldBe(3);
    }

    [Fact]
    public void TryGetValue_PromotesEntryToMostRecentlyUsed()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.TryGetValue("a", out _).ShouldBeTrue();
        cache.Set("c", 3);

        cache.TryGetValue("a", out _).ShouldBeTrue();
        cache.TryGetValue("b", out _).ShouldBeFalse();
    }

    [Fact]
    public void Set_UpdatesExistingKeyWithoutEviction()
    {
        var cache = new LruCache<string, int>(2);
        cache.Set("a", 1);
        cache.Set("b", 2);
        cache.Set("a", 10);

        cache.TryGetValue("a", out var a).ShouldBeTrue();
        a.ShouldBe(10);
        cache.TryGetValue("b", out _).ShouldBeTrue();
    }

    [Fact]
    public void NullValues_AreCached()
    {
        var cache = new LruCache<string, string?>(2);
        cache.Set("a", null);

        cache.TryGetValue("a", out var value).ShouldBeTrue();
        value.ShouldBeNull();
    }
}
