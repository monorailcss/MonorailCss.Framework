using System.Text.RegularExpressions;

namespace MonorailCss.Discovery;

/// <summary>
/// Reads source files and extracts CSS-utility-class candidates.
/// <para>
/// Three extraction strategies, picked by extension via <see cref="ScanFile"/>:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="ScanMarkup"/> — pulls <c>class="…"</c> / <c>class='…'</c> attribute
///     values out of HTML-shaped files (<c>.razor</c>, <c>.cshtml</c>, <c>.html</c>, etc.) and
///     hands the contents to the framework validator.</description></item>
///   <item><description><see cref="ScanCSharp"/> — pulls every double-quoted string literal
///     out of a <c>.cs</c> file and validates each token. Catches utilities passed via
///     <c>$"…"</c> interpolation, attribute helpers, etc.</description></item>
///   <item><description><see cref="ScanGeneric"/> — feeds the entire file content to
///     <see cref="CandidateLexer"/>. Used for content types where neither the markup nor C#
///     strategies fit (Markdown, Vue/Svelte/JSX SFCs, etc.) — there's no single
///     <c>class="…"</c> shape to anchor on, so we accept noisier extraction in exchange for
///     coverage.</description></item>
/// </list>
/// <para>
/// Used at runtime by the discovery service to bridge the hot-reload gap (when EnC deltas
/// don't rewrite the on-disk PE), and at build time by <c>MonorailCss.Build.Tasks</c> to scan
/// content sources declared via <c>@source</c>. Same primitives, same set of extracted
/// candidates, regardless of execution model.
/// </para>
/// </summary>
public sealed partial class SourceFileScanner
{
    [GeneratedRegex("class\\s*=\\s*\"([^\"]*)\"|class\\s*=\\s*'([^']*)'")]
    private static partial Regex ClassAttributeRegex();

    [GeneratedRegex("\"((?:[^\"\\\\]|\\\\.)*)\"")]
    private static partial Regex StringLiteralRegex();

    private readonly ValidationCache _validationCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="SourceFileScanner"/> class.
    /// </summary>
    /// <param name="validationCache">Shared validator. Pooled across files so the parser only
    /// runs once per unique token.</param>
    public SourceFileScanner(ValidationCache validationCache)
    {
        _validationCache = validationCache;
    }

    /// <summary>
    /// Reads <paramref name="path"/> and dispatches to the appropriate extraction strategy by
    /// file extension. Unknown extensions fall through to <see cref="ScanGeneric"/>. Read errors
    /// are silently swallowed; the caller's downstream pipeline simply sees no candidates from
    /// the unreadable file.
    /// </summary>
    /// <param name="path">Absolute path to the source file.</param>
    /// <param name="output">Destination for extracted+validated candidates.</param>
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
        switch (ext)
        {
            // Pure markup: a single <c>class="…"</c> attribute regex is precise enough.
            case ".html":
            case ".htm":
            case ".vbhtml":
            case ".aspx":
            case ".ascx":
            case ".master":
                ScanMarkup(content, output);
                break;

            // .razor and .cshtml mix markup with embedded C# (verbatim strings, @code blocks,
            // interpolation), so the markup regex misses utilities living inside C# string
            // literals — Index.razor's `@"class=""grid foo"""` lexes as a doubled-quote attribute
            // with empty content. ScanGeneric runs the whole file through CandidateLexer, which
            // catches them all (along with the occasional false positive from a comment).
            case ".razor":
            case ".cshtml":
                ScanGeneric(content, output);
                break;

            case ".cs":
                ScanCSharp(content, output);
                break;

            default:
                ScanGeneric(content, output);
                break;
        }
    }

    /// <summary>
    /// Extracts every <c>class="…"</c> or <c>class='…'</c> attribute value from
    /// <paramref name="content"/> and feeds it through the framework validator.
    /// </summary>
    /// <param name="content">Raw file content.</param>
    /// <param name="output">Destination for extracted+validated candidates.</param>
    public void ScanMarkup(string content, ICollection<string> output)
    {
        foreach (Match m in ClassAttributeRegex().Matches(content))
        {
            var raw = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;
            _validationCache.CollectValid(raw, output);
        }
    }

    /// <summary>
    /// Extracts every double-quoted string literal from <paramref name="content"/> and feeds
    /// it through the framework validator. Conservative — most non-class strings won't
    /// validate, so the noise gets filtered out by the parser.
    /// </summary>
    /// <param name="content">Raw file content.</param>
    /// <param name="output">Destination for extracted+validated candidates.</param>
    public void ScanCSharp(string content, ICollection<string> output)
    {
        foreach (Match m in StringLiteralRegex().Matches(content))
        {
            _validationCache.CollectValid(m.Groups[1].Value, output);
        }
    }

    /// <summary>
    /// Feeds the entire file content to <see cref="CandidateLexer"/>. Used for file types where
    /// neither the markup nor C# strategy applies — Markdown, MDX, Vue/Svelte SFCs, JSX/TSX,
    /// any other content type the consumer wants to scan opportunistically.
    /// </summary>
    /// <param name="content">Raw file content.</param>
    /// <param name="output">Destination for extracted+validated candidates.</param>
    public void ScanGeneric(string content, ICollection<string> output)
    {
        _validationCache.CollectValid(content, output);
    }
}
