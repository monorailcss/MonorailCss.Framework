using System.Buffers;

namespace MonorailCss.Discovery;

/// <summary>
/// Shared candidate-class tokenizer. Walks a string and yields substrings that look like
/// CSS-utility classes — leading-char restricted, balanced-bracket inside <c>[...]</c>, stopping
/// at the first HTML/code boundary. Used by <c>AssemblyClassScanner</c> for raw IL #US strings,
/// by <c>SourceFileScanner</c> for .razor/.cs literals, and by the build-task pipeline for
/// PE-image and source-file scans; each layer adds its own length and validation filtering on
/// top of the raw token stream.
/// <para>
/// The enumerator exposes each token both as a <see cref="Enumerator.CurrentSpan"/>
/// (allocation-free) and as a <see cref="Enumerator.Current"/> string (materialized on access).
/// Hot callers should prefer the span and only allocate when a token actually needs to escape.
/// </para>
/// </summary>
public static class CandidateLexer
{
    // The characters a candidate token can begin with. Digits are included so variant prefixes
    // like `2xl:`, `3xl:`, `2xs:` aren't decapitated to `xl:` / `xs:`. Wrapping the set in
    // SearchValues turns "skip ahead to the next possible token start" into a single vectorized
    // IndexOfAny call instead of a char-at-a-time loop over whitespace / prose.
    private static readonly SearchValues<char> CandidateStartChars =
        SearchValues.Create("abcdefghijklmnopqrstuvwxyz0123456789-!@[*");

    /// <summary>
    /// Tokenizes <paramref name="raw"/> into a sequence of candidate substrings. Returns a
    /// foreach-friendly value; iterate it with <c>foreach (var token in CandidateLexer.Tokenize(raw))</c>.
    /// </summary>
    /// <param name="raw">A raw string (an IL user-string entry, a class attribute value, an
    /// arbitrary literal) to scan for class-shaped substrings.</param>
    /// <returns>An enumerable sequence of candidate token substrings.</returns>
    public static TokenSequence Tokenize(string raw) => new(raw);

    /// <summary>
    /// Foreach-friendly wrapper over the lexer state. Returned by <see cref="Tokenize"/>; use
    /// <c>foreach</c> to iterate, or call <see cref="GetEnumerator"/> manually.
    /// </summary>
    public readonly struct TokenSequence
    {
        private readonly string _raw;

        internal TokenSequence(string raw) => _raw = raw;

        /// <summary>
        /// Returns an enumerator that walks the candidate tokens.
        /// </summary>
        /// <returns>A new enumerator positioned before the first token.</returns>
        public Enumerator GetEnumerator() => new(_raw);
    }

    /// <summary>
    /// Mutable enumerator state for <see cref="TokenSequence"/>. Don't construct directly —
    /// the foreach pattern handles allocation. Each call to <see cref="MoveNext"/> advances
    /// to the next candidate token; <see cref="Current"/> / <see cref="CurrentSpan"/> expose it.
    /// </summary>
    public struct Enumerator
    {
        private readonly string _raw;
        private int _i;
        private int _tokenStart;
        private int _tokenEnd;

        internal Enumerator(string raw)
        {
            _raw = raw;
            _i = 0;
            _tokenStart = 0;
            _tokenEnd = 0;
        }

        /// <summary>
        /// Gets the current candidate token as a span over the source string. Allocation-free.
        /// Valid only after <see cref="MoveNext"/> returns true.
        /// </summary>
        public readonly ReadOnlySpan<char> CurrentSpan => _raw.AsSpan(_tokenStart, _tokenEnd - _tokenStart);

        /// <summary>
        /// Gets the current candidate token as a string. Materializes a substring on each
        /// access. Valid only after <see cref="MoveNext"/> returns true.
        /// </summary>
        public readonly string Current => _raw[_tokenStart.._tokenEnd];

        /// <summary>
        /// Advances to the next candidate token in the source string.
        /// </summary>
        /// <returns>True when a token was found; false at end-of-input.</returns>
        public bool MoveNext()
        {
            // Razor compiles adjacent static markup into a single AddMarkupContent("<div class=\"foo bar\">...")
            // string literal. A whitespace-only split would leak HTML fragments like `class="foo` and
            // `bar">content</div>` into the candidate stream. Instead we scan for class-shaped runs:
            // start at any candidate-leading char, advance through the safe character set OUTSIDE
            // brackets and through anything BALANCED inside [...], and stop at the first HTML/code
            // boundary.
            var raw = _raw;
            while (_i < raw.Length)
            {
                // Fast-skip: jump straight to the next char a candidate could start with,
                // racing past runs of whitespace / prose in one vectorized scan.
                var next = raw.AsSpan(_i).IndexOfAny(CandidateStartChars);
                if (next < 0)
                {
                    _i = raw.Length;
                    return false;
                }

                _i += next;

                var start = _i;
                var depth = 0;
                while (_i < raw.Length)
                {
                    var c = raw[_i];

                    if (depth > 0)
                    {
                        if (c == '[')
                        {
                            depth++;
                        }
                        else if (c == ']')
                        {
                            depth--;
                        }

                        _i++;
                        continue;
                    }

                    if (c == '[')
                    {
                        depth = 1;
                        _i++;
                        continue;
                    }

                    if (!IsCandidateChar(c))
                    {
                        break;
                    }

                    _i++;
                }

                if (_i == start)
                {
                    // Single-char start that didn't extend; advance to avoid spinning.
                    _i++;
                    continue;
                }

                if (depth != 0)
                {
                    // Unbalanced [...]; drop the candidate without emitting it.
                    continue;
                }

                _tokenStart = start;
                _tokenEnd = _i;
                return true;
            }

            return false;
        }
    }

    private static bool IsCandidateChar(char c)
    {
        // Class-name chars OUTSIDE brackets. Inside [...] anything balanced goes; this set
        // excludes characters that only appear in arbitrary values (`>`, `=`, `&`, `<`, quotes,
        // parens, commas, semicolons) and never in a bare utility token.
        return (c is >= 'a' and <= 'z')
            || (c is >= 'A' and <= 'Z')
            || (c is >= '0' and <= '9')
            || c is '-' or '_' or ':' or '.' or '/' or '!' or '%' or '*' or '#' or '@' or '~' or '$';
    }
}
