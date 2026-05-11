using System.Collections.Concurrent;

namespace MonorailCss.Discovery;

/// <summary>
/// Memoizes <see cref="CssFramework.TryValidateCandidate(string)"/> results so the parser only
/// runs once per unique token across the entire app's lifetime — even when the same token
/// shows up in the IL scan, the source watcher, and a hot-reload rescan. Lookups are lock-free;
/// the framework reference is captured once at construction so it can't shift underneath us.
/// </summary>
public sealed class ValidationCache
{
    private readonly CssFramework _framework;
    private readonly ConcurrentDictionary<string, bool> _cache = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationCache"/> class.
    /// </summary>
    /// <param name="framework">The framework whose <see cref="CssFramework.TryValidateCandidate(string)"/>
    /// is consulted for tokens not already in the cache.</param>
    public ValidationCache(CssFramework framework)
    {
        _framework = framework;
    }

    /// <summary>
    /// Validates a single token against the framework, returning the cached result on subsequent calls.
    /// </summary>
    /// <param name="token">A single class candidate (no whitespace).</param>
    /// <returns>True when the token parses as a known utility; false otherwise.</returns>
    public bool TryValidate(string token) => _cache.GetOrAdd(token, _framework.TryValidateCandidate);

    /// <summary>
    /// Gets the number of distinct tokens currently memoized.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Tokenizes <paramref name="raw"/> via <see cref="CandidateLexer"/>, drops anything outside
    /// the 2..96 length window, and adds every validating token to <paramref name="output"/>.
    /// Owns the post-tokenize loop so every scanner — IL, source-file, build-time PE — stays in
    /// lockstep without copy-pasting the filter.
    /// </summary>
    /// <param name="raw">A raw string fragment (an IL user-string entry, a class attribute value,
    /// an arbitrary literal) to extract candidates from.</param>
    /// <param name="output">Destination for any tokens that pass validation.</param>
    public void CollectValid(string raw, ICollection<string> output)
    {
        foreach (var token in CandidateLexer.Tokenize(raw))
        {
            if (token.Length is < 2 or > 96)
            {
                continue;
            }

            if (!TryValidate(token))
            {
                continue;
            }

            output.Add(token);
        }
    }
}
