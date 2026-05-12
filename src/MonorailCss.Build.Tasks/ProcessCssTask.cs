using System.Diagnostics;
using System.IO.Abstractions;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using MonorailCss.Build.Tasks.BuildCache;
using MonorailCss.Build.Tasks.Parsing;
using MonorailCss.Build.Tasks.Scanning;
using MonorailCss.Discovery;
using MonorailCss.Parser.SourceCss;

namespace MonorailCss.Build.Tasks;

/// <summary>
/// MSBuild task that scans content files + referenced assemblies for utility classes and
/// writes the generated CSS to disk. Thin wrapper over
/// <see cref="MonorailCssGenerator"/>: this task's job is to translate MSBuild inputs
/// (<c>@(ReferencePath)</c>, <c>@source</c>-driven globs, etc.) into a generator request and
/// persist the result. Discovery and Build.Tasks share the generator, so build-time and
/// runtime emit the same CSS for the same inputs.
/// </summary>
/// <remarks>
/// Supported CSS directives in the input file (parsed by <c>CssSourceProcessor</c>):
/// <c>@import</c>, <c>@theme</c>, <c>@apply</c>, <c>@utility</c>, <c>@custom-variant</c>,
/// <c>@source "path"</c>, <c>@source not "path"</c>, <c>@source inline("…")</c>.
/// </remarks>
[UsedImplicitly]
public class ProcessCssTask : Microsoft.Build.Utilities.Task
{
    private readonly IFileSystem _fileSystem;
    private readonly GlobScanner _globScanner;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessCssTask"/> class.
    /// This parameterless constructor is required by MSBuild.
    /// </summary>
    public ProcessCssTask()
        : this(null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessCssTask"/> class.
    /// </summary>
    /// <param name="fileSystem">The file system abstraction to use for output writes and the
    /// up-to-date check. Defaults to the real file system. Note that the generator itself
    /// reads input files via real I/O regardless.</param>
    public ProcessCssTask(IFileSystem? fileSystem)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _globScanner = new GlobScanner(_fileSystem);
    }

    /// <summary>
    /// Gets or sets the input CSS file. Drives the framework configuration via
    /// <c>@theme</c>, <c>@apply</c>, <c>@utility</c>, <c>@custom-variant</c>, and tells the
    /// task what to scan via <c>@source</c> directives.
    /// </summary>
    [Required]
    public string InputFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the output CSS file to generate.
    /// </summary>
    [Required]
    public string OutputFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the files written by this task for incremental build tracking. Lets
    /// MSBuild track generated files for Clean and prevents duplicate registration in
    /// Static Web Assets.
    /// </summary>
    [Output]
    public ITaskItem[]? FileWrites { get; set; }

    /// <summary>
    /// Gets or sets the resolved assembly references the consuming project depends on,
    /// populated from <c>@(ReferencePath)</c> by the targets file. Every non-BCL,
    /// non-excluded assembly here is scanned via the same IL <c>#US</c> heap walk runtime
    /// discovery uses, giving the build task feature parity for utilities baked into
    /// component-library DLLs (BlazorMonaco, Pennington, etc.).
    /// </summary>
    public ITaskItem[]? ReferencePaths { get; set; }

    /// <summary>
    /// Gets or sets assembly names (no <c>.dll</c>, no version) to skip when scanning
    /// <see cref="ReferencePaths"/>. Populated from
    /// <c>@(MonorailCssExcludeAssembly)</c>. Mirrors
    /// <c>MonorailDiscoveryOptions.ExcludeAssemblies</c> on the runtime side.
    /// </summary>
    public ITaskItem[]? ExcludeAssemblies { get; set; }

    /// <summary>
    /// Gets or sets the build configuration (e.g., "Debug", "Release"). Used to resolve
    /// <c>$(Configuration)</c> placeholders in <c>@source</c> paths.
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    /// Gets or sets the target framework (e.g., "net9.0"). Used to resolve
    /// <c>$(TargetFramework)</c> placeholders in <c>@source</c> paths.
    /// </summary>
    public string? TargetFramework { get; set; }

    /// <summary>
    /// Gets or sets the runtime identifier (e.g., "win-x64"). Used to resolve
    /// <c>$(RuntimeIdentifier)</c> placeholders in <c>@source</c> paths.
    /// </summary>
    public string? RuntimeIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the directory used to persist per-assembly (MVID-keyed) and per-source-file
    /// (mtime-keyed) scan caches across builds. Set from the targets file to
    /// <c>$(IntermediateOutputPath)MonorailCss</c>. When unset, the task runs cold every time —
    /// fine for one-shot CI builds, slow on incremental rebuilds when MSBuild does decide to
    /// re-enter the target.
    /// </summary>
    public string? CacheDirectory { get; set; }

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
        "**/*.mdx",
    ];

    /// <summary>
    /// Executes the task.
    /// </summary>
    /// <returns>True on success.</returns>
    public override bool Execute()
    {
        try
        {
            // No inner up-to-date short-circuit: MSBuild's Inputs/Outputs metadata on the
            // ProcessMonorailCss target already gates this, and the previous mtime check here
            // only compared InputFile vs OutputFile — so a .razor/.cs edit would trigger the
            // target then immediately bail "up to date" and leave generated CSS stale. The
            // persistent cache (PersistentGenerationCache) handles the precise short-circuit
            // inside the generator instead, keyed by MVID + per-file mtime.
            Log.LogMessage(MessageImportance.Normal, $"MonorailCss: Processing {InputFile}");

            var rootDir = Path.GetDirectoryName(InputFile);
            if (string.IsNullOrEmpty(rootDir))
            {
                rootDir = _fileSystem.Directory.GetCurrentDirectory();
            }

            // The generator needs to parse the source CSS for theme/apply/utility/variant,
            // and we *also* need the parsed @source directives to decide what to scan.
            // Run the parser once here, hand the result back to the generator via BaseFramework
            // — no, actually simplest path: let the generator parse the source CSS itself
            // (single source of truth) and surface the SourceConfiguration on the result.
            // We still need to know @source directives BEFORE Generate runs, so we do a cheap
            // pre-parse for the directive set only.
            var sourceConfig = ParseSourceConfiguration();

            var sourceFiles = ResolveSourceFiles(rootDir, sourceConfig);
            var assemblyFiles = ResolveAssemblyFiles(rootDir, sourceConfig);
            var excludes = BuildExcludeSet();
            var safelist = sourceConfig.InlineSources.SelectMany(s => s.ExpandedUtilities).ToList();

            Log.LogMessage(
                MessageImportance.Normal,
                $"MonorailCss: scanning {assemblyFiles.Count} assembly reference(s), {sourceFiles.Count} source file(s)");

            var generator = new MonorailCssGenerator();

            // Persistent cache seed: load per-assembly + per-source-file scan results from the
            // previous build, hand them to the generator, and only the files actually changed
            // (by mtime) will be re-scanned this run.
            var persistentCache = string.IsNullOrEmpty(CacheDirectory)
                ? null
                : new PersistentGenerationCache(CacheDirectory);

            var seedSw = Stopwatch.StartNew();
            var seed = persistentCache?.TryLoad();
            if (seed is not null)
            {
                generator.SeedCache(seed);
                Log.LogMessage(
                    MessageImportance.Low,
                    $"MonorailCss: seeded cache from {persistentCache!.CacheFilePath} ({seed.SourceFiles.Count} source, {seed.Assemblies.Count} assembly entries, {seedSw.ElapsedMilliseconds}ms)");
            }

            var result = generator.Generate(new MonorailCssGenerationRequest
            {
                SourceCssPath = _fileSystem.File.Exists(InputFile) ? InputFile : null,
                AssemblyFiles = assemblyFiles,
                SourceFiles = sourceFiles,
                ExtraSafelist = safelist,
                ExcludeAssemblies = excludes,
            });

            if (result.Classes.Count == 0)
            {
                Log.LogWarning("No utility classes found");
            }
            else
            {
                Log.LogMessage(MessageImportance.Normal, $"Found {result.Classes.Count} unique utility classes");
            }

            EnsureOutputDirectory();
            _fileSystem.File.WriteAllText(OutputFile, result.Css);

            Log.LogMessage(
                MessageImportance.High,
                $"MonorailCss: Generated {OutputFile} ({_fileSystem.FileInfo.New(OutputFile).Length / 1024.0:F1} KB, {result.Classes.Count} classes)");

            // Persist the updated cache for the next invocation. Filter source-file entries to
            // the current scan list so deleted / no-longer-included files drop out.
            if (persistentCache is not null)
            {
                var saveSw = Stopwatch.StartNew();
                var snapshot = generator.SnapshotCache(sourceFiles);
                persistentCache.Save(snapshot);
                Log.LogMessage(
                    MessageImportance.Low,
                    $"MonorailCss: saved cache ({snapshot.SourceFiles.Count} source, {snapshot.Assemblies.Count} assembly entries, {saveSw.ElapsedMilliseconds}ms)");

                FileWrites = [
                    new Microsoft.Build.Utilities.TaskItem(OutputFile),
                    new Microsoft.Build.Utilities.TaskItem(persistentCache.CacheFilePath),
                ];
            }
            else
            {
                FileWrites = [new Microsoft.Build.Utilities.TaskItem(OutputFile)];
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"Error processing CSS: {ex.Message}");
            Log.LogMessage(MessageImportance.Low, ex.ToString());
            return false;
        }
    }

    private void EnsureOutputDirectory()
    {
        var outputDir = Path.GetDirectoryName(OutputFile);
        if (!string.IsNullOrEmpty(outputDir) && !_fileSystem.Directory.Exists(outputDir))
        {
            _fileSystem.Directory.CreateDirectory(outputDir);
        }
    }

    /// <summary>
    /// Pre-parses the input CSS for <c>@source</c> / <c>@source not</c> /
    /// <c>@source inline()</c> / <c>@import</c> directives so the task can do path
    /// resolution and glob expansion before handing the actual file/DLL lists to the
    /// generator. The generator parses the same file again for theme/apply/utility/variant
    /// — duplication is cheap and avoids weaving the SourceConfiguration through the
    /// generator's request/response.
    /// </summary>
    private SourceConfiguration ParseSourceConfiguration()
    {
        if (!_fileSystem.File.Exists(InputFile))
        {
            return new SourceConfiguration();
        }

        var content = _fileSystem.File.ReadAllText(InputFile);
        return new CssSourceParser().Parse(content);
    }

    // Framework assemblies that always contain utility-class-shaped strings (templates,
    // canonical-test fixtures, prose defaults, etc.) and would inflate the candidate set if
    // scanned. Seeded unconditionally so consumers don't have to repeat them in every csproj.
    // Microsoft.* and System.* are filtered separately by IlMetadataScanner.IsKnownFrameworkAssembly.
    private static readonly string[] _frameworkAssemblies =
    [
        "MonorailCss",
        "MonorailCss.Build.Tasks",
        "MonorailCss.Discovery",
    ];

    internal HashSet<string> BuildExcludeSet()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in _frameworkAssemblies)
        {
            set.Add(name);
        }

        if (ExcludeAssemblies is null)
        {
            return set;
        }

        foreach (var item in ExcludeAssemblies)
        {
            var name = item.ItemSpec?.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                set.Add(name);
            }
        }

        return set;
    }

    /// <summary>
    /// Resolves every source-file path the task should hand to the generator: the explicit
    /// <c>@source "path"</c> entries (with glob expansion) plus, when no explicit includes
    /// are set, an auto-detected sweep of the input file's directory (or the
    /// <c>source(…)</c> base path) using <see cref="_defaultContentPatterns"/>. Honors
    /// <c>@source not "path"</c> exclusions for both modes.
    /// </summary>
    private List<string> ResolveSourceFiles(string rootDir, SourceConfiguration config)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var baseDir = rootDir;
        if (!string.IsNullOrEmpty(config.BasePath))
        {
            baseDir = Path.IsPathRooted(config.BasePath)
                ? config.BasePath
                : Path.GetFullPath(Path.Combine(rootDir, config.BasePath));
        }

        var excludePaths = config.ExcludeSources
            .Select(s => ResolvePath(s.Path, rootDir))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var excludeDirs = excludePaths
            .Where(_fileSystem.Directory.Exists)
            .Select(p => Path.GetFileName(p))
            .Where(n => !string.IsNullOrEmpty(n))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        excludeDirs.Add("bin");
        excludeDirs.Add("obj");

        var nonDllIncludes = config.IncludeSources.Where(s => !s.IsDll).ToArray();

        if (nonDllIncludes.Length > 0)
        {
            foreach (var source in nonDllIncludes)
            {
                var resolved = ResolvePath(source.Path, rootDir);
                AddPathToFileSet(resolved, excludePaths, excludeDirs, files);
            }
        }
        else if (!config.DisableAutoDetection)
        {
            foreach (var pattern in _defaultContentPatterns)
            {
                foreach (var file in EnumerateMatching(baseDir, pattern, excludeDirs))
                {
                    files.Add(file);
                }
            }
        }

        return files.Select(Path.GetFullPath).ToList();
    }

    /// <summary>
    /// Resolves every DLL the task should hand to the generator: explicit
    /// <c>@source "path/to/Library.dll"</c> entries plus the <see cref="ReferencePaths"/>
    /// supplied by MSBuild's <c>@(ReferencePath)</c>. Build-time exclusions and BCL
    /// filtering live in the generator; this method just collects the candidate paths.
    /// </summary>
    private List<string> ResolveAssemblyFiles(string rootDir, SourceConfiguration config)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in config.IncludeSources.Where(s => s.IsDll))
        {
            var resolved = ResolvePath(source.Path, rootDir);
            files.Add(resolved);
        }

        if (ReferencePaths is not null)
        {
            foreach (var item in ReferencePaths)
            {
                if (!string.IsNullOrEmpty(item.ItemSpec))
                {
                    files.Add(item.ItemSpec);
                }
            }
        }

        return files.ToList();
    }

    private string ResolvePath(string path, string rootDir)
    {
        var resolved = PathPlaceholderResolver.ResolvePlaceholders(path, Configuration, TargetFramework, RuntimeIdentifier);
        if (PathPlaceholderResolver.ContainsPlaceholders(resolved))
        {
            Log.LogWarning($"Path contains unresolved placeholders: {resolved}");
        }

        return Path.IsPathRooted(resolved) ? resolved : Path.GetFullPath(Path.Combine(rootDir, resolved));
    }

    private void AddPathToFileSet(string path, HashSet<string> excludePaths, HashSet<string> excludeDirs, HashSet<string> files)
    {
        if (excludePaths.Contains(path))
        {
            return;
        }

        if (GlobScanner.IsGlobPattern(path))
        {
            var (baseDir, pattern) = SplitGlobPath(path);
            if (!_fileSystem.Directory.Exists(baseDir))
            {
                Log.LogWarning($"Base directory not found for glob pattern: {baseDir}");
                return;
            }

            foreach (var file in _globScanner.ExpandGlob(baseDir, pattern, excludeDirs))
            {
                files.Add(file);
            }
        }
        else if (_fileSystem.File.Exists(path))
        {
            files.Add(path);
        }
        else if (_fileSystem.Directory.Exists(path))
        {
            foreach (var pattern in _defaultContentPatterns)
            {
                foreach (var file in EnumerateMatching(path, pattern, excludeDirs))
                {
                    files.Add(file);
                }
            }
        }
        else
        {
            Log.LogWarning($"Source path not found: {path}");
        }
    }

    private IEnumerable<string> EnumerateMatching(string rootDir, string pattern, HashSet<string> excludeDirs)
    {
        if (!_fileSystem.Directory.Exists(rootDir))
        {
            return [];
        }

        var searchPattern = pattern.Replace("**\\", string.Empty).Replace("**/", string.Empty).Replace("**", "*");
        var option = pattern.Contains("**") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        try
        {
            var files = _fileSystem.Directory.GetFiles(rootDir, searchPattern, option);
            return files.Where(file =>
            {
                var relativePath = Path.GetRelativePath(rootDir, file);
                var pathParts = relativePath.Split(Path.DirectorySeparatorChar);
                return !pathParts.Any(part => excludeDirs.Contains(part));
            });
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Error scanning for pattern {pattern}: {ex.Message}");
            return [];
        }
    }

    private static (string BaseDir, string Pattern) SplitGlobPath(string path)
    {
        var parts = path.Replace('\\', '/').Split('/');
        var basePathParts = new List<string>();

        for (var i = 0; i < parts.Length; i++)
        {
            if (GlobScanner.IsGlobPattern(parts[i]))
            {
                var pattern = string.Join("/", parts[i..]);
                var baseDir = basePathParts.Count > 0
                    ? string.Join(Path.DirectorySeparatorChar, basePathParts)
                    : ".";
                return (baseDir, pattern);
            }

            basePathParts.Add(parts[i]);
        }

        return (path, "**/*");
    }
}
