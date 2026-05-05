using System.Text.RegularExpressions;

namespace MonorailCss.Discovery;

/// <summary>
/// Reads source files (.razor, .cshtml, .cs) and extracts CSS-utility-class candidates.
/// Used during dev to bridge the hot-reload gap: when the runtime applies an EnC delta,
/// the on-disk PE doesn't change, so the IL scanner can't see new strings. The file
/// watcher picks them up directly from source.
/// </summary>
internal sealed partial class SourceFileScanner
{
    [GeneratedRegex("class\\s*=\\s*\"([^\"]*)\"|class\\s*=\\s*'([^']*)'")]
    private static partial Regex ClassAttributeRegex();

    [GeneratedRegex("\"((?:[^\"\\\\]|\\\\.)*)\"")]
    private static partial Regex StringLiteralRegex();

    private readonly ValidationCache _validationCache;

    public SourceFileScanner(ValidationCache validationCache)
    {
        _validationCache = validationCache;
    }

    public void ScanFile(string path, ICollection<string> output)
    {
        string content;
        try
        {
            content = File.ReadAllText(path);
        }
        catch
        {
            return;
        }

        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext is ".razor" or ".cshtml" or ".html")
        {
            ScanMarkup(content, output);
        }
        else if (ext is ".cs")
        {
            ScanCSharp(content, output);
        }
    }

    private void ScanMarkup(string content, ICollection<string> output)
    {
        foreach (Match m in ClassAttributeRegex().Matches(content))
        {
            var raw = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
            ScanRaw(raw, output);
        }
    }

    private void ScanCSharp(string content, ICollection<string> output)
    {
        // Conservative: scan every string literal's content. Most non-class strings won't validate.
        foreach (Match m in StringLiteralRegex().Matches(content))
        {
            var raw = m.Groups[1].Value;
            ScanRaw(raw, output);
        }
    }

    private void ScanRaw(string raw, ICollection<string> output)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return;
        }

        // Same lexer as AssemblyClassScanner, kept in sync intentionally.
        var i = 0;
        while (i < raw.Length)
        {
            var startChar = raw[i];
            if (!IsCandidateStart(startChar))
            {
                i++;
                continue;
            }

            var start = i;
            var depth = 0;
            while (i < raw.Length)
            {
                var c = raw[i];

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

                    i++;
                    continue;
                }

                if (c == '[')
                {
                    depth = 1;
                    i++;
                    continue;
                }

                if (!IsCandidateChar(c))
                {
                    break;
                }

                i++;
            }

            if (depth != 0 || i == start)
            {
                if (i == start)
                {
                    i++;
                }

                continue;
            }

            var token = raw[start..i];
            if (token.Length is < 2 or > 96)
            {
                continue;
            }

            if (!_validationCache.TryValidate(token))
            {
                continue;
            }

            output.Add(token);
        }
    }

    private static bool IsCandidateStart(char c)
    {
        // Mirrors AssemblyClassScanner.IsCandidateStart; digits are valid starts so that
        // variant prefixes like `2xl:`, `3xl:`, `2xs:` begin a token instead of being skipped.
        return (c is >= 'a' and <= 'z')
            || (c is >= '0' and <= '9')
            || c is '-' or '!' or '@' or '[' or '*';
    }

    private static bool IsCandidateChar(char c)
    {
        return (c is >= 'a' and <= 'z')
            || (c is >= 'A' and <= 'Z')
            || (c is >= '0' and <= '9')
            || c is '-' or '_' or ':' or '.' or '/' or '!' or '%' or '*' or '#' or '@' or '~' or '$';
    }
}
