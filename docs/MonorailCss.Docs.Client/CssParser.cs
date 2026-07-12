using System.Text.RegularExpressions;

namespace MonorailCss.Docs.Client;

/// <summary>
/// Extracts utility class names from the HTML the user types, and detects whether the
/// caret sits inside a <c>class="…"</c> attribute (for the per-class CSS hover). Ported
/// from tests/TryMonorail.
/// </summary>
internal static partial class CssParser
{
    [GeneratedRegex("""
                    class\s*=\s*(['"])(?<value>.*?)\1
                    """, RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex FindCssClassRegex();

    [GeneratedRegex("<.*class\\s*=\\s*['|\"]([^\"]*)?$", RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex IsInCssClassReg();

    public static bool IsInCssClass(string html)
    {
        return IsInCssClassReg().IsMatch(html);
    }

    public static IEnumerable<string> GetCssClasses(string html)
    {
        var matches = FindCssClassRegex().Matches(html);
        var results = new string[matches.Count];
        for (var i = 0; i < matches.Count; i++)
        {
            results[i] = matches[i].Groups["value"].Captures[0].Value;
        }

        return results;
    }
}
