using System.Collections.Concurrent;

namespace MonorailCss.Discovery;

/// <summary>
/// Memoizes <see cref="CssFramework.TryValidateCandidate(string)"/> results so the parser only
/// runs once per unique token across the entire app's lifetime — even when the same token
/// shows up in the IL scan, the source watcher, and a hot-reload rescan. Lookups are lock-free;
/// the framework reference is captured once at construction so it can't shift underneath us.
/// </summary>
internal sealed class ValidationCache
{
    private readonly CssFramework _framework;
    private readonly ConcurrentDictionary<string, bool> _cache = new(StringComparer.Ordinal);

    public ValidationCache(CssFramework framework)
    {
        _framework = framework;
    }

    public bool TryValidate(string token) => _cache.GetOrAdd(token, _framework.TryValidateCandidate);

    public int Count => _cache.Count;
}
