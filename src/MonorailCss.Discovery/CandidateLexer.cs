namespace MonorailCss.Discovery;

/// <summary>
/// Shared candidate-class tokenizer. Walks a string and yields substrings that look like
/// CSS-utility classes — leading-char restricted, balanced-bracket inside <c>[...]</c>, stopping
/// at the first HTML/code boundary. Used by <see cref="AssemblyClassScanner"/> for raw IL #US
/// strings and <see cref="SourceFileScanner"/> for .razor/.cs literals; both layer their own
/// length and validation filtering on top of the raw token stream.
/// </summary>
internal static class CandidateLexer
{
    /// <summary>
    /// Tokenizes <paramref name="raw"/> into a sequence of candidate substrings. Returns a
    /// foreach-friendly value; iterate it with <c>foreach (var token in CandidateLexer.Tokenize(raw))</c>.
    /// </summary>
    public static TokenSequence Tokenize(string raw) => new(raw);

    public readonly struct TokenSequence
    {
        private readonly string _raw;

        internal TokenSequence(string raw) => _raw = raw;

        public Enumerator GetEnumerator() => new(_raw);
    }

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

        public string Current { get; private set; }

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
