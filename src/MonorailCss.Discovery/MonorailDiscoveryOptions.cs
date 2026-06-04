namespace MonorailCss.Discovery;

/// <summary>
/// Configuration for the MonorailCss discovery runtime. Renamed from <c>MonorailCssOptions</c>
/// so embedders that wrap this library (Pennington, etc.) can keep their own consumer-facing
/// <c>MonorailCssOptions</c> type without a namespace alias clash.
/// </summary>
public sealed class MonorailDiscoveryOptions
{
    private CssFramework? _framework;

    /// <summary>
    /// Gets or sets the framework instance used for class validation and CSS generation.
    /// Replace this when you need a custom theme, prose customization, or registered utilities.
    /// Constructed on first read so consumers that override via source CSS don't pay for the
    /// default instance.
    /// </summary>
    public CssFramework Framework
    {
        get => _framework ??= new CssFramework();
        set => _framework = value;
    }

    /// <summary>
    /// Gets a list of class names to always include in the generated CSS, regardless of
    /// whether they were discovered in any assembly. Use this for runtime-constructed
    /// classes (e.g. <c>$"bg-{color}-500"</c>) that static IL scanning cannot reconstruct.
    /// </summary>
    public List<string> ExtraSafelist { get; } = new();

    /// <summary>
    /// Gets the set of assembly names to skip during scanning. Names match exactly against
    /// <c>Assembly.GetName().Name</c>. Use this to exclude assemblies whose IL-embedded
    /// strings are not real utilities used by the running app — e.g. icon packs that bake
    /// hundreds of class-shaped strings into their metadata, or any third-party library
    /// whose discovery contribution is noise.
    /// BCL assemblies (<c>System.*</c>, <c>Microsoft.*</c>, etc.) and assemblies marked
    /// <c>[assembly: MonorailCssNoScan]</c> — including the MonorailCss framework assemblies —
    /// are skipped automatically and do not need to be listed here.
    /// </summary>
    public HashSet<string> ExcludeAssemblies { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets an in-memory CSS source. Anything you would normally put in <c>app.css</c>
    /// — <c>@theme</c> blocks, <c>@apply</c> components, <c>@utility</c> definitions,
    /// <c>@custom-variant</c> directives, plain CSS — goes here. Discovery runs this through
    /// <see cref="MonorailCss.Parser.SourceCss.CssSourceProcessor"/> to populate the framework's
    /// theme, applies, custom utilities, and custom variants; whatever the processor doesn't
    /// consume is emitted verbatim ahead of the generated utilities.
    /// <para>
    /// When you need <c>@import</c> resolution from disk, set <see cref="SourceCssPath"/> instead
    /// (or in addition — both can be set, in which case <see cref="SourceCss"/> wins for the
    /// inline content and <see cref="SourceCssPath"/>'s directory becomes the base for
    /// <c>@import</c> resolution).
    /// </para>
    /// </summary>
    public string? SourceCss { get; set; }

    /// <summary>
    /// Gets or sets the path to a CSS file that drives the framework configuration. When set,
    /// Discovery follows <c>@import</c> directives recursively, watches every imported file in
    /// development for hot-reload, and resolves relative paths against the entry file's
    /// directory. Auto-detected from <c>wwwroot/app.css</c> when both this and
    /// <see cref="SourceCss"/> are unset.
    /// </summary>
    public string? SourceCssPath { get; set; }

    /// <summary>
    /// Gets or sets the URL path the middleware serves the generated CSS at.
    /// Default: <c>/_monorail/app.css</c>.
    /// </summary>
    public string CssEndpoint { get; set; } = "/_monorail/app.css";

    /// <summary>
    /// Gets or sets an optional disk path to mirror the generated CSS to. When set, every
    /// regeneration writes the CSS atomically to this path. Useful when downstream tools
    /// (e.g. CDN uploaders, bundlers) want the file on disk rather than via HTTP.
    /// </summary>
    public string? WriteToFile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether discovery scans assets shipped by referenced
    /// packages / Razor Class Libraries as static web assets (the
    /// <c>_content/&lt;Package&gt;/…</c> set) for utility classes. These files — JavaScript that
    /// builds markup at runtime, most commonly — live in the NuGet package cache, never enter
    /// the assembly's IL, and are not inside any watched source directory, so neither the IL
    /// scan nor the source-file scan can see them. When enabled (the default), the entry
    /// assembly's <c>{name}.staticwebassets.runtime.json</c> manifest is read once at startup,
    /// every enumerated asset whose extension is in <see cref="StaticWebAssetExtensions"/> is
    /// resolved to its physical path, and those files are scanned through the same tokenizer
    /// the source-file scan uses. Packages listed in <see cref="ExcludeAssemblies"/> are
    /// skipped here too (matched against the <c>_content/&lt;Package&gt;</c> segment).
    /// </summary>
    public bool ScanStaticWebAssets { get; set; } = true;

    /// <summary>
    /// Gets the file extensions (each including the leading dot) eligible for static-web-asset
    /// scanning when <see cref="ScanStaticWebAssets"/> is enabled. Defaults to <c>.js</c> and
    /// <c>.mjs</c>. Kept deliberately narrow — an RCL's <c>_content</c> also carries CSS,
    /// images, fonts, and source maps that have no utility classes worth extracting.
    /// </summary>
    public HashSet<string> StaticWebAssetExtensions { get; } =
        new(StringComparer.OrdinalIgnoreCase) { ".js", ".mjs" };

    /// <summary>
    /// Gets a list of source directories to watch for utility-class changes during development.
    /// When non-empty, a <see cref="FileSystemWatcher"/> watches each directory recursively for
    /// <c>.razor</c>, <c>.cshtml</c>, and <c>.cs</c> changes, then re-extracts class strings
    /// directly from the source file. This bridges the hot-reload gap: <c>dotnet watch</c>
    /// applies EnC deltas in memory but does not rewrite the on-disk PE, so the IL scanner
    /// can't observe in-session edits to the user's own project. Leave this empty in production
    /// — the IL scan at startup is sufficient when no edits are expected.
    /// </summary>
    public List<string> WatchSourceDirectories { get; } = new();
}
