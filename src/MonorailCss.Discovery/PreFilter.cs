namespace MonorailCss.Discovery;

/// <summary>
/// Cheap reject-most-strings filter before the full candidate parser. The
/// <c>AssemblyClassScanner</c> lexer already restricts tokens to a structurally-valid character
/// set, so this filter mostly just enforces sensible length and a leading-character check —
/// catching obvious junk strings without prematurely rejecting valid utilities whose root isn't
/// declared via <c>GetFunctionalRoots</c> on the utility class.
/// </summary>
internal sealed class PreFilter
{
    public PreFilter(CssFramework framework)
    {
        // Constructor kept so the service signature stays stable; the framework reference is no
        // longer needed here, but discovery callers still inject it. Future heuristics (e.g. a
        // bloom filter over the full known-class set) can hook in via the same parameter.
        _ = framework;
    }

    public bool IsPlausible(string token)
    {
        if (token.Length is < 2 or > 96)
        {
            return false;
        }

        // The lexer already accepts only class-shaped chars, so length + at-least-one
        // class-shape signal (a dash, colon, slash, or bracket) is enough to reject English
        // identifiers like `console` while letting through any class regardless of whether its
        // utility implementation declares its functional root.
        var first = token[0];
        if (first is not ((>= 'a' and <= 'z') or '-' or '!' or '@' or '[' or '*'))
        {
            return false;
        }

        // Single-word tokens (no internal structure) get more scrutiny: they can only be valid
        // utilities if they're a static name, but we don't have that lookup here. Defer to the
        // full parser — its registry lookup is O(1) for static utilities.
        return true;
    }
}
