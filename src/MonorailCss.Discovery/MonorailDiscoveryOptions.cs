namespace MonorailCss.Discovery;

/// <summary>
/// Configuration for the MonorailCss discovery runtime. Renamed from <c>MonorailCssOptions</c>
/// so embedders that wrap this library (Pennington, etc.) can keep their own consumer-facing
/// <c>MonorailCssOptions</c> type without a namespace alias clash.
/// </summary>
public sealed class MonorailDiscoveryOptions
{
    /// <summary>
    /// Gets or sets the framework instance used for class validation and CSS generation.
    /// Replace this when you need a custom theme, prose customization, or registered utilities.
    /// </summary>
    public CssFramework Framework { get; set; } = new();

    /// <summary>
    /// Gets a list of class names to always include in the generated CSS, regardless of
    /// whether they were discovered in any assembly. Use this for runtime-constructed
    /// classes (e.g. <c>$"bg-{color}-500"</c>) that static IL scanning cannot reconstruct.
    /// </summary>
    public List<string> ExtraSafelist { get; } = new();

    /// <summary>
    /// Gets or sets the CSS source supplied by the user. Anything you would normally put
    /// in <c>app.css</c> — <c>@theme</c> blocks, <c>@apply</c> components, plain CSS — goes
    /// here. The discovery output is appended to this content. Pass either inline CSS or the
    /// result of <c>File.ReadAllText("wwwroot/app.css")</c>.
    /// </summary>
    public string? SourceCss { get; set; }

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
