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
            _validationCache.CollectValid(raw, output);
        }
    }

    private void ScanCSharp(string content, ICollection<string> output)
    {
        // Conservative: scan every string literal's content. Most non-class strings won't validate.
        foreach (Match m in StringLiteralRegex().Matches(content))
        {
            _validationCache.CollectValid(m.Groups[1].Value, output);
        }
    }
}
