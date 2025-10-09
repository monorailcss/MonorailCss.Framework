using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using MonorailCss.Build.Tasks.Parsing;
using MonorailCss.Build.Tasks.Scanning;

namespace MonorailCss.Build.Tasks;

/// <summary>
/// MSBuild task that scans content files for Tailwind utility classes
/// and generates optimized CSS output using the MonorailCss framework.
/// </summary>
/// <remarks>
/// Supported CSS directives:
/// - @import with source(), theme(), and layer() modifiers (theme/layer parsed but not yet implemented)
/// - @source for explicit file/directory inclusion
/// - @source not for exclusion
/// - @source inline() for safelisting utilities
/// - @custom-variant for defining custom pseudo-class/element variants
/// - @utility for custom utility definitions
/// - @theme for CSS custom properties
/// - @apply for component composition
/// </remarks>
[UsedImplicitly]
public partial class ProcessCssTask : Microsoft.Build.Utilities.Task
{
    private readonly IFileSystem _fileSystem;
    private readonly DllScanner _dllScanner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessCssTask"/> class.
    /// </summary>
    /// <param name="fileSystem">The file system abstraction to use for file operations. Defaults to the real file system if not provided.</param>
    public ProcessCssTask(IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _dllScanner = new DllScanner(_fileSystem);
    }

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

    // Default content patterns for common web frameworks
    private static readonly string[] _defaultContentPatterns =
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
                rootDir = _fileSystem.Directory.GetCurrentDirectory();
            }

            // Parse the input CSS file first to get source configuration
            var baseTheme = new MonorailCss.Theme.Theme();
            var baseApplies = ImmutableDictionary<string, string>.Empty;
            var customUtilities = ImmutableList<MonorailCss.Parser.Custom.UtilityDefinition>.Empty;
            var customVariants = ImmutableList<CustomVariantDefinition>.Empty;
            var sourceConfiguration = new SourceConfiguration();

            if (_fileSystem.File.Exists(InputFile))
            {
                var cssContent = _fileSystem.File.ReadAllText(InputFile);
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

                // Convert and include custom utilities from @utility blocks
                if (parsedData.UtilityDefinitions.Any())
                {
                    Log.LogMessage(MessageImportance.Low, $"Found {parsedData.UtilityDefinitions.Count} custom utilities");
                    customUtilities = parsedData.UtilityDefinitions
                        .Select(ConvertToFrameworkDefinition)
                        .ToImmutableList();
                }

                // Convert and include custom variants from @custom-variant directives
                if (parsedData.SourceConfiguration.CustomVariants.Any())
                {
                    Log.LogMessage(MessageImportance.Low, $"Found {parsedData.SourceConfiguration.CustomVariants.Count} custom variants");
                    customVariants = parsedData.SourceConfiguration.CustomVariants
                        .Select(cv => new CustomVariantDefinition
                        {
                            Name = cv.Name,
                            Selector = cv.Selector,
                            Weight = 490 // Default weight before built-in pseudo-elements
                        })
                        .ToImmutableList();
                }

                // Log parsed @import directives
                if (parsedData.SourceConfiguration.Imports.Any())
                {
                    Log.LogMessage(MessageImportance.Low, $"Found {parsedData.SourceConfiguration.Imports.Count} @import directives");
                    foreach (var import in parsedData.SourceConfiguration.Imports)
                    {
                        var modifierInfo = import.Modifier != ImportModifier.None
                            ? $" {import.Modifier.ToString().ToLowerInvariant()}({import.ModifierValue})"
                            : string.Empty;
                        Log.LogMessage(MessageImportance.Low, $"  @import \"{import.Path}\"{modifierInfo}");
                    }
                }

                // Get source configuration
                sourceConfiguration = parsedData.SourceConfiguration;
            }

            // Scan for utilities using source configuration (or auto-detect if no config)
            var utilities = ScanWithSourceConfiguration(rootDir, sourceConfiguration);

            if (utilities.Count == 0)
            {
                Log.LogWarning("No utility classes found in content files");
            }
            else
            {
                Log.LogMessage(MessageImportance.Normal, $"Found {utilities.Count} unique utility classes");
            }

            // Create settings with the configured theme, custom utilities, custom variants, and always include preflight
            var settings = new CssFrameworkSettings
            {
                IncludePreflight = true,
                Theme = baseTheme,
                Applies = baseApplies,
                CustomUtilities = customUtilities,
                CustomVariants = customVariants
            };

            // Create and configure the CSS framework
            var framework = new CssFramework(settings);

            // Process the utilities
            var combinedUtilities = string.Join(" ", utilities);
            var generatedCss = framework.Process(combinedUtilities);

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(OutputFile);
            if (!string.IsNullOrEmpty(outputDir) && !_fileSystem.Directory.Exists(outputDir))
            {
                _fileSystem.Directory.CreateDirectory(outputDir);
            }

            // Write output
            _fileSystem.File.WriteAllText(OutputFile, generatedCss);

            Log.LogMessage(MessageImportance.High, $"MonorailCss: Generated {OutputFile} ({_fileSystem.FileInfo.New(OutputFile).Length / 1024.0:F1} KB)");

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
    /// Scans files using @source and @import directives from the input CSS.
    /// </summary>
    private HashSet<string> ScanWithSourceConfiguration(string rootDir, SourceConfiguration config)
    {
        var utilities = new HashSet<string>();

        // Determine the base directory for scanning
        var baseDir = rootDir;
        if (!string.IsNullOrEmpty(config.BasePath))
        {
            baseDir = Path.IsPathRooted(config.BasePath)
                ? config.BasePath
                : Path.GetFullPath(Path.Combine(rootDir, config.BasePath));
            Log.LogMessage(MessageImportance.Low, $"Using base path: {baseDir}");
        }

        // Build exclude list from @source not directives
        var excludePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var exclude in config.ExcludeSources)
        {
            var resolvedPath = Path.IsPathRooted(exclude.Path)
                ? exclude.Path
                : Path.GetFullPath(Path.Combine(rootDir, exclude.Path));
            excludePaths.Add(resolvedPath);
            Log.LogMessage(MessageImportance.Low, $"Excluding: {resolvedPath}");
        }

        // Process explicit @source includes
        if (config.IncludeSources.Count > 0)
        {
            Log.LogMessage(MessageImportance.Normal, $"Processing {config.IncludeSources.Count} explicit source(s)");
            foreach (var source in config.IncludeSources)
            {
                var resolvedPath = Path.IsPathRooted(source.Path)
                    ? source.Path
                    : Path.GetFullPath(Path.Combine(rootDir, source.Path));

                if (source.IsDll)
                {
                    // Handle DLL scanning
                    var dllUtilities = _dllScanner.ScanDllForUtilities(resolvedPath, Log);
                    foreach (var utility in dllUtilities)
                    {
                        utilities.Add(utility);
                    }
                }
                else
                {
                    // Scan directory or file
                    ScanSourcePath(resolvedPath, utilities, excludePaths);
                }
            }
        }
        else if (!config.DisableAutoDetection)
        {
            // Auto-detect: scan baseDir with default patterns
            Log.LogMessage(MessageImportance.Normal, "Auto-detecting sources from base directory");
            var excludeDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bin", "obj" };

            // Add exclude paths as directory names
            foreach (var excludePath in excludePaths)
            {
                var dirName = Path.GetFileName(excludePath);
                if (!string.IsNullOrEmpty(dirName))
                {
                    excludeDirs.Add(dirName);
                }
            }

            utilities = ScanContentFiles(baseDir, _defaultContentPatterns, excludeDirs);
        }

        // Process inline safelisted utilities
        foreach (var inline in config.InlineSources)
        {
            Log.LogMessage(MessageImportance.Low, $"Adding inline utilities: {inline.Pattern}");
            foreach (var utility in inline.ExpandedUtilities)
            {
                utilities.Add(utility);
            }
        }

        return utilities;
    }

    /// <summary>
    /// Scans a specific source path (file or directory) for utilities.
    /// </summary>
    private void ScanSourcePath(string path, HashSet<string> utilities, HashSet<string> excludePaths)
    {
        // Check if path is excluded
        if (excludePaths.Contains(path))
        {
            Log.LogMessage(MessageImportance.Low, $"Skipping excluded path: {path}");
            return;
        }

        if (_fileSystem.File.Exists(path))
        {
            // Scan single file
            try
            {
                var content = _fileSystem.File.ReadAllText(path);
                var classNames = ExtractClassNames(content);
                foreach (var className in classNames)
                {
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
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Could not read file {path}: {ex.Message}");
            }
        }
        else if (_fileSystem.Directory.Exists(path))
        {
            // Scan directory with default patterns
            var excludeDirs = excludePaths
                .Where(_fileSystem.Directory.Exists)
                .Select(p => Path.GetFileName(p))
                .Where(name => !string.IsNullOrEmpty(name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            excludeDirs.Add("bin");
            excludeDirs.Add("obj");

            var scannedUtilities = ScanContentFiles(path, _defaultContentPatterns, excludeDirs);
            foreach (var utility in scannedUtilities)
            {
                utilities.Add(utility);
            }
        }
        else
        {
            Log.LogWarning($"Source path not found: {path}");
        }
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
                    var content = _fileSystem.File.ReadAllText(file);
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
                var files = _fileSystem.Directory.GetFiles(rootDir, searchPattern, option);

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
                var files = _fileSystem.Directory.GetFiles(rootDir, pattern, SearchOption.TopDirectoryOnly);

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
                return [];
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

    /// <summary>
    /// Converts a parsed utility definition from the build task parser to the framework's utility definition format.
    /// </summary>
    private MonorailCss.Parser.Custom.UtilityDefinition ConvertToFrameworkDefinition(ParsedUtilityDefinition parsedDefinition)
    {
        // Convert parsed CSS declarations to framework CSS declarations
        var declarations = parsedDefinition.Declarations
            .Select(d => new MonorailCss.Parser.Custom.CssDeclaration(d.Property, d.Value))
            .ToImmutableList();

        // Convert nested selectors
        var nestedSelectors = parsedDefinition.NestedSelectors
            .Select(ns => new MonorailCss.Parser.Custom.NestedSelector(
                ns.Selector,
                ns.Declarations.Select(d => new MonorailCss.Parser.Custom.CssDeclaration(d.Property, d.Value)).ToImmutableList()))
            .ToImmutableList();

        return new MonorailCss.Parser.Custom.UtilityDefinition
        {
            Pattern = parsedDefinition.Pattern,
            IsWildcard = parsedDefinition.IsWildcard,
            Declarations = declarations,
            NestedSelectors = nestedSelectors,
            CustomPropertyDependencies = parsedDefinition.CustomPropertyDependencies
        };
    }

}