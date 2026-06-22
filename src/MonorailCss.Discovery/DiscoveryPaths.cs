namespace MonorailCss.Discovery;

/// <summary>
/// Shared path policy for the discovery runtime: which directories are never worth walking
/// into, and which file extensions carry utility classes worth scanning. Centralised here so
/// the recursive source-file walk (<see cref="ClassDiscoveryService"/>) and the referenced-
/// project root resolver (<see cref="ProjectSourceRootResolver"/>) apply identical rules and
/// can't drift apart.
/// </summary>
internal static class DiscoveryPaths
{
    /// <summary>
    /// Directory names that are never worth walking into. Mirrors Tailwind v4's
    /// <c>ignored-content-dirs.txt</c> (<c>B:\tailwindcss\crates\oxide\fixtures</c>) plus
    /// the .NET-specific <c>bin</c>/<c>obj</c> and Visual Studio's <c>.vs</c>. Matched as
    /// segment names anywhere in the path. <c>.gitignore</c> parsing is intentionally not
    /// implemented; this hard-coded set covers the 95% case without pulling in a parser.
    /// </summary>
    public static readonly HashSet<string> IgnoredDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", ".hg", ".svn", ".jj",
        ".vs", ".idea", ".vscode",
        "bin", "obj",
        "node_modules", ".next", ".nuxt", ".svelte-kit", ".parcel-cache",
        ".turbo", ".vercel", ".pnpm-store", ".yarn",
        ".venv", "venv", "__pycache__",
    };

    /// <summary>
    /// True when <paramref name="path"/> carries an extension the source-file scan extracts
    /// utility classes from (<c>.razor</c>, <c>.cshtml</c>, <c>.cs</c>, <c>.html</c>).
    /// </summary>
    /// <param name="path">A file path.</param>
    /// <returns>True when the extension is one of the scanned source types.</returns>
    public static bool HasSupportedExtension(string path)
    {
        var ext = Path.GetExtension(path);
        return ext.Equals(".razor", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".cshtml", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".cs", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".html", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// True when any segment of <paramref name="path"/> is an
    /// <see cref="IgnoredDirectoryNames"/> entry. Walks path segments without allocating
    /// through <see cref="Path.GetDirectoryName(string)"/> in a loop — a single string scan
    /// suffices.
    /// </summary>
    /// <param name="path">A file or directory path.</param>
    /// <returns>True when the path lies inside an ignored directory.</returns>
    public static bool IsInIgnoredDirectory(string path)
    {
        var span = path.AsSpan();
        var start = 0;
        for (var i = 0; i <= span.Length; i++)
        {
            if (i == span.Length || span[i] == Path.DirectorySeparatorChar || span[i] == Path.AltDirectorySeparatorChar)
            {
                if (i > start)
                {
                    var segment = span.Slice(start, i - start).ToString();
                    if (IgnoredDirectoryNames.Contains(segment))
                    {
                        return true;
                    }
                }

                start = i + 1;
            }
        }

        return false;
    }
}
