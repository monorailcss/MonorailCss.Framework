using System.Buffers;
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
    // ASCII letters only. A token built entirely from these can never be a variant-prefixed,
    // valued, negative, important, modified, or arbitrary utility — every one of those forms
    // introduces a non-letter character — so a valid all-letters token is necessarily a bare
    // static utility or a bare functional root. That makes the all-letters test a sound,
    // allocation-free pre-filter when checked against _knownNames (see CollectValid).
    private static readonly SearchValues<char> AsciiLetters =
        SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");

    private readonly CssFramework _framework;
    private readonly ConcurrentDictionary<string, bool> _cache = new(StringComparer.Ordinal);

    // Span keyed view over _cache. Lets CollectValid probe the memoization table with a
    // ReadOnlySpan<char> straight off the lexer, so a token that's already been seen never
    // costs a substring allocation. StringComparer.Ordinal supplies the span->string bridge.
    private readonly ConcurrentDictionary<string, bool>.AlternateLookup<ReadOnlySpan<char>> _spanCache;

    // The framework's bare static utility names (flex, block, ...) plus functional roots
    // (bg, text, content, ...), span-probeable. An all-letters token absent from this set
    // cannot be a utility, so the pre-filter can drop it without parsing; see CollectValid.
    private readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _knownNames;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationCache"/> class.
    /// </summary>
    /// <param name="framework">The framework whose <see cref="CssFramework.TryValidateCandidate(string)"/>
    /// is consulted for tokens not already in the cache.</param>
    public ValidationCache(CssFramework framework)
    {
        _framework = framework;
        _spanCache = _cache.GetAlternateLookup<ReadOnlySpan<char>>();

        var knownNames = new HashSet<string>(framework.GetStaticUtilityNames(), StringComparer.Ordinal);
        knownNames.UnionWith(framework.GetFunctionalUtilityPrefixes());
        _knownNames = knownNames.GetAlternateLookup<ReadOnlySpan<char>>();
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
        var tokens = CandidateLexer.Tokenize(raw).GetEnumerator();
        while (tokens.MoveNext())
        {
            var span = tokens.CurrentSpan;
            if (span.Length is < 2 or > 96)
            {
                continue;
            }

            // Structural pre-filter: an all-letters token can only be a valid utility if its
            // text is a known static utility or functional root. This rejects natural-language
            // prose — the bulk of the noise in markdown/HTML/identifiers — with one vectorized
            // scan and one set probe, before any cache lookup, parse, or allocation. It is
            // conservative by construction: it can only reject tokens the parser also rejects.
            if (!span.ContainsAnyExcept(AsciiLetters) && !_knownNames.Contains(span))
            {
                continue;
            }

            if (_spanCache.TryGetValue(span, out var valid))
            {
                // Already-seen token: most prose / identifier noise lands here on its second
                // sighting onward and never allocates. Only validating tokens cost a string.
                if (valid)
                {
                    output.Add(span.ToString());
                }
            }
            else
            {
                // First sighting: the parser needs a string regardless, so materialize once.
                // GetOrAdd seeds the cache and absorbs the concurrent-miss race.
                var token = span.ToString();
                if (_cache.GetOrAdd(token, _framework.TryValidateCandidate))
                {
                    output.Add(token);
                }
            }
        }
    }
}
