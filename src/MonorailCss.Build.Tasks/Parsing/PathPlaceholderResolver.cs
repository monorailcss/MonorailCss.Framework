using System.Text.RegularExpressions;

namespace MonorailCss.Build.Tasks.Parsing;

/// <summary>
/// Resolves placeholders in file paths using MSBuild property values.
/// Supports common MSBuild properties like $(Configuration), $(TargetFramework), and $(RuntimeIdentifier).
/// </summary>
internal static partial class PathPlaceholderResolver
{
    /// <summary>
    /// Replaces placeholders in a path with actual values from MSBuild properties.
    /// Placeholders are case-insensitive and follow the format $(PropertyName).
    /// </summary>
    /// <param name="path">The path containing placeholders (e.g., "../bin/$(Configuration)/$(TargetFramework)/MyLib.dll").</param>
    /// <param name="configuration">The build configuration (e.g., "Debug", "Release"). Can be null.</param>
    /// <param name="targetFramework">The target framework (e.g., "net9.0", "net8.0"). Can be null.</param>
    /// <param name="runtimeIdentifier">The runtime identifier (e.g., "win-x64", "linux-x64"). Can be null.</param>
    /// <returns>The path with all placeholders replaced. If a placeholder value is null, the placeholder is left unchanged.</returns>
    /// <example>
    /// <code>
    /// var resolved = ResolvePlaceholders(
    ///     "../bin/$(Configuration)/$(TargetFramework)/MyLib.dll",
    ///     "Debug",
    ///     "net9.0",
    ///     null);
    /// // Result: "../bin/Debug/net9.0/MyLib.dll"
    /// </code>
    /// </example>
    public static string ResolvePlaceholders(
        string path,
        string? configuration,
        string? targetFramework,
        string? runtimeIdentifier)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        // Replace $(Configuration) with the actual configuration value
        if (!string.IsNullOrEmpty(configuration))
        {
            path = ConfigurationPlaceholderRegex().Replace(path, configuration);
        }

        // Replace $(TargetFramework) with the actual target framework value
        if (!string.IsNullOrEmpty(targetFramework))
        {
            path = TargetFrameworkPlaceholderRegex().Replace(path, targetFramework);
        }

        // Replace $(RuntimeIdentifier) with the actual runtime identifier value
        if (!string.IsNullOrEmpty(runtimeIdentifier))
        {
            path = RuntimeIdentifierPlaceholderRegex().Replace(path, runtimeIdentifier);
        }

        return path;
    }

    /// <summary>
    /// Checks if a path contains any unresolved placeholders.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path contains placeholders like $(PropertyName), false otherwise.</returns>
    public static bool ContainsPlaceholders(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return PlaceholderDetectionRegex().IsMatch(path);
    }

    // Case-insensitive regex for $(Configuration)
    [GeneratedRegex(@"\$\(Configuration\)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ConfigurationPlaceholderRegex();

    // Case-insensitive regex for $(TargetFramework)
    [GeneratedRegex(@"\$\(TargetFramework\)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex TargetFrameworkPlaceholderRegex();

    // Case-insensitive regex for $(RuntimeIdentifier)
    [GeneratedRegex(@"\$\(RuntimeIdentifier\)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RuntimeIdentifierPlaceholderRegex();

    // Regex to detect any placeholder pattern $(Name)
    [GeneratedRegex(@"\$\([^)]+\)", RegexOptions.Compiled)]
    private static partial Regex PlaceholderDetectionRegex();
}
