using System.Collections.Immutable;

namespace MonorailCss.Build.Tasks.Parsing;

/// <summary>
/// Represents the complete source configuration parsed from @import and @source directives.
/// </summary>
internal record SourceConfiguration
{
    /// <summary>
    /// Gets the base path for automatic source detection from @import source(...).
    /// Null means use default (input file directory).
    /// </summary>
    public string? BasePath { get; init; }

    /// <summary>
    /// Gets a value indicating whether automatic source detection is disabled.
    /// Set to true when @import source(none) is specified.
    /// </summary>
    public bool DisableAutoDetection { get; init; }

    /// <summary>
    /// Gets the list of explicit source paths to include.
    /// </summary>
    public ImmutableList<SourceDirective> IncludeSources { get; init; } = ImmutableList<SourceDirective>.Empty;

    /// <summary>
    /// Gets the list of source paths to exclude.
    /// </summary>
    public ImmutableList<SourceDirective> ExcludeSources { get; init; } = ImmutableList<SourceDirective>.Empty;

    /// <summary>
    /// Gets the list of inline safelisted utilities.
    /// </summary>
    public ImmutableList<InlineSourceDirective> InlineSources { get; init; } = ImmutableList<InlineSourceDirective>.Empty;

    /// <summary>
    /// Gets a value indicating whether any source configuration was found.
    /// </summary>
    public bool HasConfiguration =>
        BasePath != null ||
        DisableAutoDetection ||
        IncludeSources.Count > 0 ||
        ExcludeSources.Count > 0 ||
        InlineSources.Count > 0;
}

/// <summary>
/// Represents a parsed @source directive or @import source() parameter.
/// </summary>
internal record SourceDirective
{
    /// <summary>
    /// Gets the path or glob pattern to include or exclude.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this is a DLL file reference.
    /// </summary>
    public bool IsDll => Path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the resolved absolute path (set during processing).
    /// </summary>
    public string? ResolvedPath { get; init; }
}

/// <summary>
/// Represents an inline source directive for safelisting utilities.
/// Example: @source inline("underline") or @source inline("bg-red-{50,100,200}")
/// </summary>
internal record InlineSourceDirective
{
    /// <summary>
    /// Gets the raw utility pattern to safelist.
    /// May contain brace expansion like "bg-red-{50,100,200}" or variant groups like "{hover:,focus:,}underline".
    /// </summary>
    public string Pattern { get; init; } = string.Empty;

    /// <summary>
    /// Gets the expanded list of utility classes after processing braces and variants.
    /// </summary>
    public ImmutableList<string> ExpandedUtilities { get; init; } = ImmutableList<string>.Empty;
}
