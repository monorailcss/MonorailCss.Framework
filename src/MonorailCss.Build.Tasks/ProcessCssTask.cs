using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using MonorailCss.Build.Tasks.Parsing;

namespace MonorailCss.Build.Tasks;

/// <summary>
/// MSBuild task that scans content files for Tailwind utility classes
/// and generates optimized CSS output using the MonorailCss framework.
/// </summary>
public partial class ProcessCssTask : Microsoft.Build.Utilities.Task
{
    /// <summary>
    /// Gets or sets the input CSS file that contains theme definitions and custom utilities.
    /// </summary>
    [Required]
    public string InputFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the output CSS file to generate.
    /// </summary>
    [Required]
    public string OutputFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the glob patterns for content files to scan.
    /// If not specified, uses default patterns.
    /// </summary>
    public string? ContentPatterns { get; set; }

    /// <summary>
    /// Gets or sets the directories to exclude from scanning.
    /// Default is "bin;obj".
    /// </summary>
    public string ExcludePaths { get; set; } = "bin;obj";

    // Default content patterns for common web frameworks
    private static readonly string[] DefaultContentPatterns =
    [
        "**/*.html",
        "**/*.htm",
        "**/*.razor",
        "**/*.cshtml",
        "**/*.vbhtml",
        "**/*.aspx",
        "**/*.ascx",
        "**/*.master",
        "**/*.jsx",
        "**/*.tsx",
        "**/*.vue",
        "**/*.svelte",
        "**/*.md",
        "**/*.mdx"
    ];

    // Regex patterns for extracting class names from content
    [GeneratedRegex(@"class\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex ClassAttributeRegex();

    [GeneratedRegex(@"className\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex ClassNameAttributeRegex();

    [GeneratedRegex(@":class\s*=\s*[""']([^""']*)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex VueClassAttributeRegex();

    [GeneratedRegex(@"@class\s*\(\s*[""']([^""']*)[""']\s*\)", RegexOptions.IgnoreCase)]
    private static partial Regex BlazorClassAttributeRegex();

    [GeneratedRegex(@"classList\s*=\s*\{([^}]*)\}", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ClassListObjectRegex();

    [GeneratedRegex(@"[""']([^""']*)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex StringLiteralRegex();

    /// <summary>
    /// Executes the task to process CSS utilities.
    /// </summary>
    /// <returns>True if the task executed successfully; otherwise, false.</returns>
    public override bool Execute()
    {
        try
        {
            Log.LogMessage(MessageImportance.Normal, $"MonorailCss: Processing {InputFile}");

            // Determine the root directory from the input file location
            var rootDir = Path.GetDirectoryName(InputFile);
            if (string.IsNullOrEmpty(rootDir))
            {
                rootDir = Directory.GetCurrentDirectory();
            }

            // Get content file patterns
            var patterns = GetContentPatterns();

            // Get excluded directories
            var excludeDirs = ExcludePaths.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Scan content files for utility classes
            var utilities = ScanContentFiles(rootDir, patterns, excludeDirs);

            if (utilities.Count == 0)
            {
                Log.LogWarning("No utility classes found in content files");
            }
            else
            {
                Log.LogMessage(MessageImportance.Normal, $"Found {utilities.Count} unique utility classes");
            }

            // Parse the input CSS file and build theme
            var baseTheme = new MonorailCss.Theme.Theme();
            var baseApplies = ImmutableDictionary<string, string>.Empty;

            if (File.Exists(InputFile))
            {
                var cssContent = File.ReadAllText(InputFile);
                var parser = new CssThemeParser();
                var parsedData = parser.Parse(cssContent);
                Log.LogMessage(MessageImportance.Low, $"Parsed theme from: {InputFile}");

                // Apply theme variables
                if (parsedData.ThemeVariables.Any())
                {
                    Log.LogMessage(MessageImportance.Low, $"Found {parsedData.ThemeVariables.Count} theme variables");
                    foreach (var (key, value) in parsedData.ThemeVariables)
                    {
                        baseTheme = baseTheme.Add(key, value);
                    }
                }

                // Apply component rules
                if (parsedData.ComponentRules.Any())
                {
                    Log.LogMessage(MessageImportance.Low, $"Found {parsedData.ComponentRules.Count} component rules");
                    baseApplies = baseApplies.SetItems(parsedData.ComponentRules);
                }

                // Note: Custom utilities from @utility blocks need special handling
                // For now, we'll pass them as CSS theme sources for the framework to process
                if (parsedData.UtilityDefinitions.Any())
                {
                    Log.LogMessage(MessageImportance.Low, $"Found {parsedData.UtilityDefinitions.Count} custom utilities");
                    // TODO: Handle custom utilities registration
                }
            }

            // Create settings with the configured theme and always include preflight
            var settings = new CssFrameworkSettings
            {
                IncludePreflight = true,
                Theme = baseTheme,
                Applies = baseApplies
            };

            // Create and configure the CSS framework
            var framework = new CssFramework(settings);

            // Process the utilities
            var combinedUtilities = string.Join(" ", utilities);
            var generatedCss = framework.Process(combinedUtilities);

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(OutputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Write output
            File.WriteAllText(OutputFile, generatedCss);

            Log.LogMessage(MessageImportance.High, $"MonorailCss: Generated {OutputFile} ({new FileInfo(OutputFile).Length / 1024.0:F1} KB)");

            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"Error processing CSS: {ex.Message}");
            Log.LogMessage(MessageImportance.Low, ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// Gets the content file patterns to use for scanning.
    /// </summary>
    private string[] GetContentPatterns()
    {
        if (!string.IsNullOrEmpty(ContentPatterns))
        {
            return ContentPatterns.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }

        return DefaultContentPatterns;
    }

    /// <summary>
    /// Scans content files for utility classes.
    /// </summary>
    private HashSet<string> ScanContentFiles(string rootDir, string[] patterns, HashSet<string> excludeDirs)
    {
        var utilities = new HashSet<string>();
        var filesScanned = 0;

        foreach (var pattern in patterns)
        {
            var files = GetMatchingFiles(rootDir, pattern, excludeDirs);

            foreach (var file in files)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    var classNames = ExtractClassNames(content);

                    foreach (var className in classNames)
                    {
                        // Split on whitespace to get individual classes
                        var classes = className.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var cls in classes)
                        {
                            var trimmed = cls.Trim();
                            if (!string.IsNullOrEmpty(trimmed))
                            {
                                utilities.Add(trimmed);
                            }
                        }
                    }

                    filesScanned++;
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"Could not read file {file}: {ex.Message}");
                }
            }
        }

        Log.LogMessage(MessageImportance.Normal, $"Scanned {filesScanned} files");
        return utilities;
    }

    /// <summary>
    /// Gets files matching a glob pattern, excluding specified directories.
    /// </summary>
    private IEnumerable<string> GetMatchingFiles(string rootDir, string pattern, HashSet<string> excludeDirs)
    {
        // Handle glob patterns
        if (pattern.Contains("**"))
        {
            var searchPattern = pattern.Replace("**\\", "").Replace("**/", "").Replace("**", "*");
            var option = pattern.Contains("**") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            try
            {
                var files = Directory.GetFiles(rootDir, searchPattern, option);

                // Filter out files in excluded directories
                return files.Where(file =>
                {
                    var relativePath = Path.GetRelativePath(rootDir, file);
                    var pathParts = relativePath.Split(Path.DirectorySeparatorChar);

                    // Check if any path part matches an excluded directory
                    return !pathParts.Any(part => excludeDirs.Contains(part));
                });
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Error scanning for pattern {pattern}: {ex.Message}");
                return Array.Empty<string>();
            }
        }
        else
        {
            try
            {
                var files = Directory.GetFiles(rootDir, pattern, SearchOption.TopDirectoryOnly);

                // Filter out files in excluded directories
                return files.Where(file =>
                {
                    var relativePath = Path.GetRelativePath(rootDir, file);
                    var pathParts = relativePath.Split(Path.DirectorySeparatorChar);

                    // Check if any path part matches an excluded directory
                    return !pathParts.Any(part => excludeDirs.Contains(part));
                });
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Error scanning for pattern {pattern}: {ex.Message}");
                return Array.Empty<string>();
            }
        }
    }

    /// <summary>
    /// Extracts class names from content using various patterns.
    /// </summary>
    private IEnumerable<string> ExtractClassNames(string content)
    {
        var classNames = new HashSet<string>();

        // Extract from class="..." attributes
        var matches = ClassAttributeRegex().Matches(content);
        foreach (Match match in matches)
        {
            classNames.Add(match.Groups[1].Value);
        }

        // Extract from className="..." attributes (React/JSX)
        matches = ClassNameAttributeRegex().Matches(content);
        foreach (Match match in matches)
        {
            classNames.Add(match.Groups[1].Value);
        }

        // Extract from :class="..." attributes (Vue)
        matches = VueClassAttributeRegex().Matches(content);
        foreach (Match match in matches)
        {
            classNames.Add(match.Groups[1].Value);
        }

        // Extract from @class(...) directives (Blazor)
        matches = BlazorClassAttributeRegex().Matches(content);
        foreach (Match match in matches)
        {
            classNames.Add(match.Groups[1].Value);
        }

        // Extract from classList objects
        matches = ClassListObjectRegex().Matches(content);
        foreach (Match match in matches)
        {
            var objectContent = match.Groups[1].Value;
            var stringMatches = StringLiteralRegex().Matches(objectContent);
            foreach (Match stringMatch in stringMatches)
            {
                classNames.Add(stringMatch.Groups[1].Value);
            }
        }

        return classNames;
    }

}