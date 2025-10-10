using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing;

namespace MonorailCss.Build.Tasks.Scanning;

/// <summary>
/// Handles glob pattern matching for file system scanning.
/// Supports wildcards (*), recursive globs (**), and brace expansion ({a,b,c}).
/// </summary>
internal partial class GlobScanner
{
    private readonly IFileSystem _fileSystem;

    public GlobScanner(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Expands a glob pattern into a list of matching file paths.
    /// </summary>
    /// <param name="baseDirectory">The base directory to search from.</param>
    /// <param name="pattern">The glob pattern (may include brace expansion).</param>
    /// <param name="excludeDirs">Directory names to exclude from results.</param>
    /// <returns>List of matching file paths.</returns>
    public IEnumerable<string> ExpandGlob(string baseDirectory, string pattern, HashSet<string> excludeDirs)
    {
        // Expand brace patterns first: {a,b,c} -> multiple patterns
        var expandedPatterns = ExpandBracePattern(pattern);

        var allMatches = new HashSet<string>();

        foreach (var expandedPattern in expandedPatterns)
        {
            var matches = MatchPattern(baseDirectory, expandedPattern, excludeDirs);
            foreach (var match in matches)
            {
                allMatches.Add(match);
            }
        }

        return allMatches;
    }

    /// <summary>
    /// Checks if a pattern contains glob syntax.
    /// </summary>
    public static bool IsGlobPattern(string path)
    {
        return path.Contains('*') || path.Contains('{') || path.Contains('}') || path.Contains('?');
    }

    /// <summary>
    /// Expands brace patterns like {a,b,c} into multiple patterns.
    /// Example: "src/{Pages,Components}/**/*.razor" -> ["src/Pages/**/*.razor", "src/Components/**/*.razor"]
    /// </summary>
    private List<string> ExpandBracePattern(string pattern)
    {
        var braceRegex = BraceExpansionRegex();
        var match = braceRegex.Match(pattern);

        if (!match.Success)
        {
            // No brace expansion needed
            return [pattern];
        }

        var prefix = pattern[..match.Index];
        var suffix = pattern[(match.Index + match.Length)..];
        var options = match.Groups[1].Value.Split(',', StringSplitOptions.TrimEntries);

        var results = new List<string>();
        foreach (var option in options)
        {
            var expanded = prefix + option + suffix;
            // Recursively expand in case of nested braces
            results.AddRange(ExpandBracePattern(expanded));
        }

        return results;
    }

    /// <summary>
    /// Matches a single glob pattern (without brace expansion) against the file system.
    /// </summary>
    private IEnumerable<string> MatchPattern(string baseDirectory, string pattern, HashSet<string> excludeDirs)
    {
        if (!_fileSystem.Directory.Exists(baseDirectory))
        {
            return [];
        }

        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        matcher.AddInclude(pattern);

        // Add exclusions for common build directories
        foreach (var excludeDir in excludeDirs)
        {
            matcher.AddExclude($"**/{excludeDir}/**");
            matcher.AddExclude($"{excludeDir}/**");
        }

        // Get all files recursively and let the matcher filter them
        var allFiles = GetAllFiles(baseDirectory, excludeDirs);
        var relativePaths = allFiles.Select(f => Path.GetRelativePath(baseDirectory, f)).ToList();

        // Execute the matcher against the relative paths
        var matchedPaths = relativePaths.Where(p => matcher.Match(p).HasMatches).ToList();

        // Convert back to absolute paths with normalized separators
        return matchedPaths.Select(p =>
        {
            var normalizedPath = p.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(baseDirectory, normalizedPath);
        });
    }

    /// <summary>
    /// Recursively gets all files in a directory, excluding specified directories.
    /// </summary>
    private List<string> GetAllFiles(string directory, HashSet<string> excludeDirs)
    {
        var files = new List<string>();

        if (!_fileSystem.Directory.Exists(directory))
        {
            return files;
        }

        try
        {
            // Add all files in current directory
            files.AddRange(_fileSystem.Directory.GetFiles(directory));

            // Recursively process subdirectories
            foreach (var subDir in _fileSystem.Directory.GetDirectories(directory))
            {
                var dirName = Path.GetFileName(subDir);
                if (!excludeDirs.Contains(dirName, StringComparer.OrdinalIgnoreCase))
                {
                    files.AddRange(GetAllFiles(subDir, excludeDirs));
                }
            }
        }
        catch
        {
            // Ignore errors accessing directories
        }

        return files;
    }

    [GeneratedRegex(@"\{([^}]+)\}")]
    private static partial Regex BraceExpansionRegex();
}
