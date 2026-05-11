using System.Collections.Concurrent;
using System.Collections.Immutable;
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
    private readonly ConcurrentDictionary<string, CachedScan> _scanCache =
        new(StringComparer.OrdinalIgnoreCase);

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
        // Per-file cache: if the on-disk mtime hasn't changed, replay the previously-extracted
        // candidate set. Without this, every regeneration trigger (late-load, hot-reload, even a
        // single-file source-watcher) re-reads + re-tokenizes hundreds of files when only one
        // (or none) actually changed.
        DateTime mtime;
        try
        {
            mtime = File.GetLastWriteTimeUtc(path);
        }
        catch
        {
            return;
        }

        if (_scanCache.TryGetValue(path, out var cached) && cached.MTime == mtime)
        {
            foreach (var token in cached.Tokens)
            {
                output.Add(token);
            }

            return;
        }

        ReadTokenizeCacheAndEmit(path, mtime, output);
    }

    /// <summary>
    /// Reads, tokenises, and caches <paramref name="path"/> unconditionally — skipping the
    /// mtime short-circuit used by <see cref="ScanFile"/>. Use this when the caller already
    /// knows the file changed (e.g. a <see cref="System.IO.FileSystemWatcher"/> just reported
    /// it) and the mtime check would just be wasted I/O.
    /// </summary>
    /// <param name="path">Absolute path to the source file.</param>
    /// <param name="output">Destination for extracted+validated candidates.</param>
    public void ScanFileForceRescan(string path, ICollection<string> output)
    {
        DateTime mtime;
        try
        {
            mtime = File.GetLastWriteTimeUtc(path);
        }
        catch
        {
            // Best-effort: if we can't stat, fall back to a sentinel mtime so a later mtime-
            // aware ScanFile call will be forced to re-read this file.
            mtime = DateTime.MinValue;
        }

        ReadTokenizeCacheAndEmit(path, mtime, output);
    }

    private void ReadTokenizeCacheAndEmit(string path, DateTime mtime, ICollection<string> output)
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

        var local = new HashSet<string>(StringComparer.Ordinal);
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
                ScanMarkup(content, local);
                break;

            // .razor and .cshtml mix markup with embedded C# (verbatim strings, @code blocks,
            // interpolation), so the markup regex misses utilities living inside C# string
            // literals — Index.razor's `@"class=""grid foo"""` lexes as a doubled-quote attribute
            // with empty content. ScanGeneric runs the whole file through CandidateLexer, which
            // catches them all (along with the occasional false positive from a comment).
            case ".razor":
            case ".cshtml":
                ScanGeneric(content, local);
                break;

            case ".cs":
                ScanCSharp(content, local);
                break;

            default:
                ScanGeneric(content, local);
                break;
        }

        _scanCache[path] = new CachedScan(mtime, local.ToImmutableArray());

        foreach (var token in local)
        {
            output.Add(token);
        }
    }

    private readonly record struct CachedScan(DateTime MTime, ImmutableArray<string> Tokens);

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
