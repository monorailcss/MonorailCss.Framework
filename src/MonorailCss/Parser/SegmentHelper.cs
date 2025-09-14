using System.Collections.Immutable;

namespace MonorailCss.Parser;

/// <summary>
/// Helper class for segmenting strings while respecting nesting of brackets, parentheses, and quotes.
/// Based on Tailwind CSS's segment function.
/// </summary>
public static class SegmentHelper
{
    private const char Backslash = '\\';
    private const char OpenCurly = '{';
    private const char CloseCurly = '}';
    private const char OpenParen = '(';
    private const char CloseParen = ')';
    private const char OpenBracket = '[';
    private const char CloseBracket = ']';
    private const char DoubleQuote = '"';
    private const char SingleQuote = '\'';

    /// <summary>
    /// Splits a string on a top-level character, respecting nesting of brackets, parentheses, and quotes.
    /// </summary>
    /// <param name="input">The string to segment.</param>
    /// <param name="separator">The character to split on.</param>
    /// <returns>Immutable list of segments.</returns>
    public static ImmutableList<string> Segment(string input, char separator)
    {
        if (string.IsNullOrEmpty(input))
        {
            return ImmutableList.Create(input);
        }

        var parts = ImmutableList.CreateBuilder<string>();
        var lastPos = 0;
        var len = input.Length;

        // Stack to track nesting level
        var closingBracketStack = new Stack<char>();

        for (var idx = 0; idx < len; idx++)
        {
            var ch = input[idx];

            // Only split on separator when at top level (stack is empty)
            if (closingBracketStack.Count == 0 && ch == separator)
            {
                parts.Add(input.Substring(lastPos, idx - lastPos));
                lastPos = idx + 1;
                continue;
            }

            switch (ch)
            {
                case Backslash:
                    // The next character is escaped, so we skip it
                    if (idx + 1 < len)
                    {
                        idx++;
                    }

                    break;

                // Handle strings - they should be processed as-is until the end
                case SingleQuote:
                case DoubleQuote:
                    var quoteChar = ch;

                    // Find the closing quote
                    while (++idx < len)
                    {
                        var nextChar = input[idx];

                        // The next character is escaped, so we skip it
                        if (nextChar == Backslash)
                        {
                            if (idx + 1 < len)
                            {
                                idx++;
                            }

                            continue;
                        }

                        if (nextChar == quoteChar)
                        {
                            break;
                        }
                    }

                    break;

                case OpenParen:
                    closingBracketStack.Push(CloseParen);
                    break;

                case OpenBracket:
                    closingBracketStack.Push(CloseBracket);
                    break;

                case OpenCurly:
                    closingBracketStack.Push(CloseCurly);
                    break;

                case CloseBracket:
                case CloseCurly:
                case CloseParen:
                    if (closingBracketStack.Count > 0 && ch == closingBracketStack.Peek())
                    {
                        closingBracketStack.Pop();
                    }

                    break;
            }
        }

        // Add the remaining part
        parts.Add(input[lastPos..]);

        return parts.ToImmutable();
    }
}