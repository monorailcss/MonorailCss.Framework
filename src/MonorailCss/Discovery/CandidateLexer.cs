namespace MonorailCss.Discovery;

/// <summary>
/// Shared candidate-class tokenizer. Walks a string and yields substrings that look like
/// CSS-utility classes — leading-char restricted, balanced-bracket inside <c>[...]</c>, stopping
/// at the first HTML/code boundary. Used by <c>AssemblyClassScanner</c> for raw IL #US strings,
/// by <c>SourceFileScanner</c> for .razor/.cs literals, and by the build-task pipeline for
/// PE-image and source-file scans; each layer adds its own length and validation filtering on
/// top of the raw token stream.
/// </summary>
public static class CandidateLexer
{
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
    /// to the next candidate token; <see cref="Current"/> exposes it.
    /// </summary>
    public struct Enumerator
    {
        private readonly string _raw;
        private int _i;

        internal Enumerator(string raw)
        {
            _raw = raw;
            _i = 0;
            Current = string.Empty;
        }

        /// <summary>
        /// Gets the current candidate token. Valid only after <see cref="MoveNext"/> returns true.
        /// </summary>
        public string Current { get; private set; }

        /// <summary>
        /// Advances to the next candidate token in the source string.
        /// </summary>
        /// <returns>True when a token was found and stored in <see cref="Current"/>; false at end-of-input.</returns>
        public bool MoveNext()
        {
            // Razor compiles adjacent static markup into a single AddMarkupContent("<div class=\"foo bar\">...")
            // string literal. A whitespace-only split would leak HTML fragments like `class="foo` and
            // `bar">content</div>` into the candidate stream. Instead we scan for class-shaped runs:
            // start at any candidate-leading char, advance through the safe character set OUTSIDE
            // brackets and through anything BALANCED inside [...], and stop at the first HTML/code
            // boundary.
            while (_i < _raw.Length)
            {
                if (!IsCandidateStart(_raw[_i]))
                {
                    _i++;
                    continue;
                }

                var start = _i;
                var depth = 0;
                while (_i < _raw.Length)
                {
                    var c = _raw[_i];

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

                Current = _raw[start.._i];
                return true;
            }

            return false;
        }
    }

    private static bool IsCandidateStart(char c)
    {
        // Digits start variant prefixes like `2xl:`, `3xl:`, `2xs:`. Without them, the lexer
        // skips the leading `2` and emits the wrong token (`xl:w-92` instead of `2xl:w-92`).
        return (c is >= 'a' and <= 'z')
            || (c is >= '0' and <= '9')
            || c is '-' or '!' or '@' or '[' or '*';
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
