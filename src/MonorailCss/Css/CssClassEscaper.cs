using System.Collections.Concurrent;
using System.Text;

namespace MonorailCss.Css;

/// <summary>
/// Escapes CSS class names to be valid CSS selectors, following Tailwind v4 escaping rules.
/// </summary>
/// <remarks>
/// CSS identifier rules (per CSS spec):
/// - Identifiers can contain letters, digits, hyphens (-), underscores (_), and escaped characters
/// - Cannot start with a digit or hyphen followed by a digit (e.g., "2xl" or "-2xl")
/// - Cannot start with two hyphens (e.g., "--var")
/// - Special characters must be escaped with a backslash (\)
/// - When escaping with hex codes, a space is added after to prevent the next character from being interpreted as part of the hex code
///
/// Examples:
/// - "2xl:bg-red" becomes "\32 xl\:bg-red" (3 is hex 33, 2 is hex 32)
/// - "-2xl:margin" becomes "-\32 xl\:margin"
/// - "hover:bg-blue" becomes "hover\:bg-blue".
/// </remarks>
internal static class CssClassEscaper
{
    private static readonly ConcurrentDictionary<string, string> _escapeCache = new();

    public static string Escape(string className)
    {
        if (string.IsNullOrEmpty(className))
        {
            return className;
        }

        return _escapeCache.GetOrAdd(className, EscapeCore);
    }

    private static string EscapeCore(string className)
    {
        var sb = new StringBuilder(className.Length * 2);

        // Handle special cases for the start of the identifier
        var startIndex = 0;
        if (className.Length > 0)
        {
            var firstChar = className[0];

            // Check if identifier starts with a digit
            if (char.IsDigit(firstChar))
            {
                // Escape the digit using hex notation with trailing space
                sb.Append('\\');
                sb.AppendFormat("{0:x} ", (int)firstChar);
                startIndex = 1;
            }

            // Check if identifier starts with hyphen followed by digit
            else if (firstChar == '-' && className.Length > 1 && char.IsDigit(className[1]))
            {
                // Keep the hyphen, escape the following digit
                sb.Append('-');
                sb.Append('\\');
                sb.AppendFormat("{0:x} ", (int)className[1]);
                startIndex = 2;
            }
        }

        // Process the rest of the string
        for (var i = startIndex; i < className.Length; i++)
        {
            var c = className[i];

            // Check if character needs escaping
            if (NeedsEscaping(c))
            {
                sb.Append('\\');

                // For special CSS characters, just add backslash
                // For others, use hex encoding
                if (IsSpecialCssChar(c))
                {
                    sb.Append(c);
                }
                else
                {
                    // Hex encode with space after for safety
                    sb.AppendFormat("{0:x} ", (int)c);
                }
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private static bool NeedsEscaping(char c)
    {
        // CSS identifier rules:
        // - Can contain letters, digits, hyphens, underscores
        // - Cannot start with digit, two hyphens, or hyphen followed by digit
        // - Special characters need escaping
        return c switch
        {
            ':' or '/' or '[' or ']' or '(' or ')' or
            '!' or '@' or '#' or '$' or '%' or '^' or
            '&' or '*' or '=' or '+' or '`' or '~' or
            '|' or '\\' or ';' or '"' or '\'' or
            '<' or '>' or '?' or '{' or '}' or
            ' ' or '\t' or '\n' or '\r' or '.' or ',' => true,
            _ => false,
        };
    }

    private static bool IsSpecialCssChar(char c)
    {
        // These characters just need a backslash, not hex encoding
        return c switch
        {
            ':' or '/' or '[' or ']' or '(' or ')' or
            '!' or '.' or ',' or '@' or '#' or '%' or
            '&' or '*' or '+' or '=' or '~' or '\'' or
            '>' => true,
            _ => false,
        };
    }

    /// <summary>
    /// Clears the escape cache. Useful for testing or memory management.
    /// </summary>
    public static void ClearCache()
    {
        _escapeCache.Clear();
    }
}